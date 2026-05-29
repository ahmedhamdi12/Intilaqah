using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class DocumentsController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IWebHostEnvironment _env;
        private readonly Cloudinary         _cloudinary;

        private static readonly string[] AllowedExtensions =
            { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public DocumentsController(IUnitOfWork uow, IWebHostEnvironment env, IConfiguration config)
        {
            _uow = uow;
            _env = env;
            
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        // ── GET /CompanyAdmin/Documents ───────────────────────────────
        public async Task<IActionResult> Index()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            var allDocs      = (await _uow.Documents.GetAllAsync()).ToList();
            var employees    = (await _uow.Employees.GetAllAsync())
                                .ToDictionary(e => e.Id, e => e.FullNameAr);
            var today        = DateTime.UtcNow.Date;

            var allMapped = allDocs.Select(d => MapToListItemVM(d, employees)).ToList();

            var expired     = allMapped.Where(d => d.DaysRemaining.HasValue && d.DaysRemaining <= 0)
                                       .OrderBy(d => d.DaysRemaining).ToList();
            var exp30       = allMapped.Where(d => d.DaysRemaining.HasValue
                                               && d.DaysRemaining > 0
                                               && d.DaysRemaining <= 30)
                                       .OrderBy(d => d.DaysRemaining).ToList();
            var exp60       = allMapped.Where(d => d.DaysRemaining.HasValue
                                               && d.DaysRemaining > 30
                                               && d.DaysRemaining <= 60)
                                       .OrderBy(d => d.DaysRemaining).ToList();
            var exp90       = allMapped.Where(d => d.DaysRemaining.HasValue
                                               && d.DaysRemaining > 60
                                               && d.DaysRemaining <= 90)
                                       .OrderBy(d => d.DaysRemaining).ToList();

            var vm = new DocumentsAlertVM
            {
                TotalDocuments  = allDocs.Count,
                ExpiredCount    = expired.Count,
                Expiring30Count = exp30.Count,
                Expiring60Count = exp60.Count,
                Expiring90Count = exp90.Count,
                ValidCount      = allMapped.Count(d =>
                    !d.DaysRemaining.HasValue || d.DaysRemaining > 90),

                ExpiredDocs    = expired,
                Expiring30Docs = exp30,
                Expiring60Docs = exp60,
                Expiring90Docs = exp90,
                CompanyDocs    = allMapped
                    .Where(d => d.EntityType == DocumentEntityType.Company)
                    .OrderBy(d => d.DaysRemaining).ToList(),
                EmployeeDocs   = allMapped
                    .Where(d => d.EntityType == DocumentEntityType.Employee)
                    .OrderBy(d => d.DaysRemaining).ToList(),
            };

            ViewBag.ExpiringDocs = expired.Count + exp30.Count;

            return View(vm);
        }

        // ── GET /CompanyAdmin/Documents/Employee?employeeId=xxx ───────
        public async Task<IActionResult> Employee(Guid employeeId)
        {
            var employee = await _uow.Employees.GetByIdAsync(employeeId);
            if (employee == null) return NotFound();

            var docs = (await _uow.Documents
                .GetByEntityAsync(employeeId, DocumentEntityType.Employee))
                .ToList();

            var employees = new Dictionary<Guid, string>
                { [employeeId] = employee.FullNameAr };

            var vm = docs.Select(d => MapToListItemVM(d, employees)).ToList();

            ViewBag.Employee = employee;
            return View(vm);
        }

        // ── GET /CompanyAdmin/Documents/Company ───────────────────────
        public async Task<IActionResult> Company()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(tenantId);
            var docs   = (await _uow.Documents
                .GetByEntityAsync(tenantId, DocumentEntityType.Company))
                .ToList();

            var entityName = new Dictionary<Guid, string>
                { [tenantId] = tenant?.Name ?? "الشركة" };

            var vm = docs.Select(d => MapToListItemVM(d, entityName)).ToList();

            ViewBag.TenantName = tenant?.Name;
            return View(vm);
        }

        // ── GET /CompanyAdmin/Documents/Upload ────────────────────────
        public async Task<IActionResult> Upload(
            Guid entityId, DocumentEntityType entityType)
        {
            var entityName = await GetEntityNameAsync(entityId, entityType);
            var vm = new DocumentUploadVM
            {
                EntityId      = entityId,
                EntityType    = entityType,
                EntityName    = entityName,
                DocTypeOptions = GetDocTypeOptions(entityType),
            };
            return View(vm);
        }

        // ── POST /CompanyAdmin/Documents/Upload ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(DocumentUploadVM model)
        {
            if (!ModelState.IsValid)
            {
                model.DocTypeOptions = GetDocTypeOptions(model.EntityType);
                model.EntityName     = await GetEntityNameAsync(
                    model.EntityId, model.EntityType);
                return View(model);
            }

            string? filePath = null;

            if (model.File != null && model.File.Length > 0)
            {
                if (!IsValidFile(model.File))
                {
                    ModelState.AddModelError("File",
                        "نوع الملف غير مسموح أو حجمه يتجاوز 5MB. المسموح: PDF, JPG, PNG");
                    model.DocTypeOptions = GetDocTypeOptions(model.EntityType);
                    model.EntityName     = await GetEntityNameAsync(
                        model.EntityId, model.EntityType);
                    return View(model);
                }

                await using var stream = model.File.OpenReadStream();
                
                // Use ImageUploadParams even for PDFs so Cloudinary allows inline previews
                // and avoids 'raw' file access restrictions.
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(model.File.FileName, stream),
                    Folder = "intilaqah/documents",
                    PublicId = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                filePath = uploadResult.SecureUrl.ToString();
            }

            var document = new Document
            {
                EntityId   = model.EntityId,
                EntityType = model.EntityType,
                DocType    = model.DocType,
                ExpiryDate = model.ExpiryDate,
                FilePath   = filePath,
                CreatedBy  = User.FindFirst("FullName")?.Value ?? "system",
            };

            await _uow.Documents.AddAsync(document);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم رفع الوثيقة بنجاح";

            return model.EntityType == DocumentEntityType.Employee
                ? RedirectToAction("Employee", new { employeeId = model.EntityId })
                : RedirectToAction("Company");
        }

        // ── GET /CompanyAdmin/Documents/Download/id ───────────────────
        // Proxies the file through the server to allow inline preview and enforce access
        public async Task<IActionResult> Download(Guid id)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null || string.IsNullOrEmpty(doc.FilePath)) return NotFound();

            try
            {
                var uri = new Uri(doc.FilePath);
                var segments = uri.AbsolutePath.Split('/');
                int uploadIndex = Array.IndexOf(segments, "upload");

                string downloadUrl = doc.FilePath;

                if (uploadIndex != -1 && segments.Length > uploadIndex + 2)
                {
                    var afterUpload = segments.Skip(uploadIndex + 1).ToArray();
                    int startIdx = (afterUpload.Length > 0 && afterUpload[0].StartsWith("v") 
                        && long.TryParse(afterUpload[0].Substring(1), out _)) ? 1 : 0;

                    // Correctly unescape the public_id to avoid double-encoding %20 -> %2520
                    var publicId = Uri.UnescapeDataString(string.Join("/", afterUpload.Skip(startIdx)));
                    var resourceType = Array.IndexOf(segments, "raw") != -1 ? "raw" : "image";

                    // Generate a signed delivery URL to bypass any strict delivery 401s
                    downloadUrl = _cloudinary.Api.UrlImgUp
                        .ResourceType(resourceType)
                        .Signed(true)
                        .BuildUrl(publicId);
                }

                using var http = new HttpClient();
                var response = await http.GetAsync(downloadUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    
                    // Force PDF to render inline in browser instead of downloading
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    if (doc.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        contentType = "application/pdf";
                    }

                    // By returning File without the fileName parameter, ASP.NET Core sends Content-Disposition: inline
                    return File(bytes, contentType);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return Content($"فشل جلب الملف من Cloudinary. الحالة: {response.StatusCode}\nالرسالة: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                return Content($"حدث خطأ أثناء الاتصال: {ex.Message}");
            }
        }


        // ── POST /CompanyAdmin/Documents/Delete/id ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, string returnUrl = "Index")
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null) return NotFound();

            if (!string.IsNullOrEmpty(doc.FilePath) && doc.FilePath.Contains("res.cloudinary.com"))
            {
                try
                {
                    var uri = new Uri(doc.FilePath);
                    var segments = uri.AbsolutePath.Split('/');
                    int uploadIndex = Array.IndexOf(segments, "upload");
                    if (uploadIndex != -1 && segments.Length > uploadIndex + 2)
                    {
                        var publicIdSegments = segments.Skip(uploadIndex + 2).ToArray();
                        var publicId = string.Join("/", publicIdSegments);
                        
                        var delParams = new DelResParams()
                        {
                            PublicIds = new List<string> { publicId },
                            ResourceType = ResourceType.Raw
                        };
                        await _cloudinary.DeleteResourcesAsync(delParams);
                    }
                }
                catch
                {
                    // Ignore parsing errors for robust deletion
                }
            }

            _uow.Documents.Delete(doc);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم حذف الوثيقة";

            return doc.EntityType == DocumentEntityType.Employee
                ? RedirectToAction("Employee", new { employeeId = doc.EntityId })
                : RedirectToAction("Company");
        }

        // ── Private Helpers ───────────────────────────────────────────
        private DocumentListItemVM MapToListItemVM(
            Document doc, Dictionary<Guid, string> entityNames)
        {
            var daysRemaining = doc.ExpiryDate.HasValue
                ? (int?)(doc.ExpiryDate.Value.Date - DateTime.UtcNow.Date).Days
                : null;

            return new DocumentListItemVM
            {
                Id            = doc.Id,
                EntityId      = doc.EntityId,
                EntityType    = doc.EntityType,
                EntityName    = entityNames.GetValueOrDefault(doc.EntityId, "—"),
                DocType       = doc.DocType,
                FilePath      = doc.FilePath,
                ExpiryDate    = doc.ExpiryDate,
                DaysRemaining = daysRemaining,
                UrgencyClass  = GetUrgencyClass(doc.ExpiryDate),
                UrgencyLabel  = GetUrgencyLabel(doc.ExpiryDate),
                HasFile       = !string.IsNullOrEmpty(doc.FilePath),
            };
        }

        private static string GetUrgencyClass(DateTime? expiryDate)
        {
            if (!expiryDate.HasValue) return "inactive";
            var days = (expiryDate.Value.Date - DateTime.UtcNow.Date).Days;
            return days <= 0  ? "danger"
                 : days <= 30 ? "danger"
                 : days <= 60 ? "warning"
                 : days <= 90 ? "info"
                 : "active";
        }

        private static string GetUrgencyLabel(DateTime? expiryDate)
        {
            if (!expiryDate.HasValue) return "بدون انتهاء";
            var days = (expiryDate.Value.Date - DateTime.UtcNow.Date).Days;
            return days <= 0  ? "منتهية"
                 : days <= 30 ? $"تنتهي خلال {days} يوم"
                 : $"{days} يوم متبقي";
        }

        private bool IsValidFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext) && file.Length <= MaxFileSizeBytes;
        }

        private IEnumerable<SelectListItem> GetDocTypeOptions(
            DocumentEntityType entityType)
        {
            var employeeTypes = new[]
            {
                "إقامة", "جواز سفر", "رخصة قيادة",
                "شهادة مهنية", "عقد عمل", "أخرى"
            };
            var companyTypes = new[]
            {
                "سجل تجاري", "رخصة بلدية", "شهادة زكاة",
                "اشتراك غرفة تجارية", "تأمين", "أخرى"
            };
            var types = entityType == DocumentEntityType.Employee
                ? employeeTypes : companyTypes;

            return types.Select(t => new SelectListItem(t, t));
        }

        private async Task<string> GetEntityNameAsync(
            Guid entityId, DocumentEntityType entityType)
        {
            if (entityType == DocumentEntityType.Employee)
            {
                var emp = await _uow.Employees.GetByIdAsync(entityId);
                return emp?.FullNameAr ?? "موظف";
            }
            var tenant = await _uow.Tenants.GetByIdAsync(entityId);
            return tenant?.Name ?? "الشركة";
        }
    }
}

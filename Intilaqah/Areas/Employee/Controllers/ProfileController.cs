using Intilaqah.Models.ViewModels.Employee;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Intilaqah.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class ProfileController : Controller
    {
        private readonly IUnitOfWork     _uow;
        private readonly Supabase.Client _supabase;
        private readonly string          _bucketName;

        private static readonly string[] AllowedImageExts =
            { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageBytes = 3 * 1024 * 1024; // 3 MB

        public ProfileController(IUnitOfWork uow, Supabase.Client supabase, IConfiguration config)
        {
            _uow        = uow;
            _supabase   = supabase;
            _bucketName = config["Supabase:BucketName"] ?? "documnts";
        }

        private async Task<Intilaqah.Models.Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return (await _uow.Employees
                .FindAsync(e => e.UserId == userId && e.IsActive))
                .FirstOrDefault();
        }

        public async Task<IActionResult> Index()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            var contract = await _uow.Contracts.GetActiveContractAsync(employee.Id);
            var departments = (await _uow.Departments.GetAllAsync())
                .ToDictionary(d => d.Id, d => d.Name);

            ViewBag.Employee   = employee;
            ViewBag.Contract   = contract;
            ViewBag.Department = employee.DepartmentId.HasValue
                ? departments.GetValueOrDefault(employee.DepartmentId.Value, "—")
                : "—";

            return View();
        }

        public async Task<IActionResult> Edit()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            var departments = (await _uow.Departments.GetAllAsync())
                .ToDictionary(d => d.Id, d => d.Name);

            var vm = new ProfileEditViewModel
            {
                FullNameAr = employee.FullNameAr,
                FullNameEn = employee.FullNameEn,
                EmployeeCode = employee.EmployeeCode,
                JobTitle = employee.JobTitle,
                Department = employee.DepartmentId.HasValue
                    ? departments.GetValueOrDefault(employee.DepartmentId.Value, "—")
                    : "—",
                Phone = employee.Phone,
                CurrentProfilePicturePath = employee.ProfilePicturePath
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            if (!ModelState.IsValid)
            {
                var departments = (await _uow.Departments.GetAllAsync())
                    .ToDictionary(d => d.Id, d => d.Name);

                model.FullNameAr  = employee.FullNameAr;
                model.FullNameEn  = employee.FullNameEn;
                model.EmployeeCode = employee.EmployeeCode;
                model.JobTitle    = employee.JobTitle;
                model.Department  = employee.DepartmentId.HasValue
                    ? departments.GetValueOrDefault(employee.DepartmentId.Value, "—")
                    : "—";
                model.CurrentProfilePicturePath = employee.ProfilePicturePath;
                return View(model);
            }

            // Only allow Phone & ProfilePicturePath edits
            employee.Phone = model.Phone;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var ext = Path.GetExtension(model.ProfilePicture.FileName).ToLowerInvariant();
                if (!AllowedImageExts.Contains(ext) || model.ProfilePicture.Length > MaxImageBytes)
                {
                    ModelState.AddModelError("ProfilePicture",
                        "نوع الصورة غير مسموح أو حجمها يتجاوز 3MB. المسموح: JPG, PNG, WEBP");
                    model.FullNameAr = employee.FullNameAr;
                    model.CurrentProfilePicturePath = employee.ProfilePicturePath;
                    return View(model);
                }

                // Delete old picture if exists
                if (!string.IsNullOrEmpty(employee.ProfilePicturePath))
                {
                    try
                    {
                        await _supabase.Storage.From(_bucketName)
                            .Remove(new List<string> { employee.ProfilePicturePath });
                    }
                    catch { /* ignore delete errors */ }
                }

                // Upload new picture
                var fileName = $"profile_{employee.Id}_{DateTime.UtcNow.Ticks}{ext}";

                await using var stream = model.ProfilePicture.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                await _supabase.Storage.From(_bucketName).Upload(
                    bytes,
                    fileName,
                    new Supabase.Storage.FileOptions
                        { ContentType = model.ProfilePicture.ContentType }
                );

                employee.ProfilePicturePath = fileName;
            }

            _uow.Employees.Update(employee);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الملف الشخصي بنجاح";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Proxy profile picture from Supabase Storage.</summary>
        [AllowAnonymous]
        public async Task<IActionResult> Avatar(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return NotFound();
            try
            {
                var bytes = await _supabase.Storage.From(_bucketName).Download(fileName, null);
                var ext   = Path.GetExtension(fileName).ToLowerInvariant();
                var ct    = ext == ".png"  ? "image/png"
                          : ext == ".webp" ? "image/webp"
                          : "image/jpeg";
                return File(bytes, ct);
            }
            catch
            {
                return NotFound();
            }
        }

    }
}

using Intilaqah.Models;
using Intilaqah.Models.ViewModels.SuperAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class CompaniesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CompaniesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var tenants = (await _uow.Tenants.GetAllAsync()).ToList();
            var plans = (await _uow.Plans.GetAllAsync()).ToDictionary(p => p.Id, p => p.Name);

            var vm = tenants.Select(t => new TenantListItemVM
            {
                Id = t.Id,
                Name = t.Name,
                CommercialRegNo = t.CommercialRegNo,
                Phone = t.Phone,
                PlanName = plans.ContainsKey(t.PlanId) ? plans[t.PlanId] : "غير محدد",
                EmployeeCount = 0, // Placeholder as per instructions
                Status = t.Status,
                NitaqatColor = t.NitaqatColor,
                ContractEndDate = t.ContractEndDate
            }).ToList();

            ViewBag.ExpiringCount = vm.Count(v => v.IsExpiringSoon);

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            var model = new TenantCreateVM();
            await PopulateAvailablePlans(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenantCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _uow.Tenants.FindAsync(t => t.CommercialRegNo == model.CommercialRegNo);
                if (existing.Any())
                {
                    ModelState.AddModelError("CommercialRegNo", "رقم السجل التجاري مسجل مسبقاً");
                }
                else
                {
                    var tenant = new Tenant
                    {
                        Id = Guid.NewGuid(),
                        TenantId = Guid.Empty, // SuperAdmin context
                        CreatedBy = User.FindFirst("FullName")?.Value ?? "system",
                        CreatedAt = DateTime.UtcNow,
                        Status = TenantStatus.Active,
                        Name = model.Name,
                        CommercialRegNo = model.CommercialRegNo,
                        Phone = model.Phone,
                        PlanId = model.PlanId,
                        ContractEndDate = model.ContractEndDate.Value,
                        NitaqatColor = model.NitaqatColor
                    };

                    await _uow.Tenants.AddAsync(tenant);
                    await _uow.SaveChangesAsync();
                    TempData["Success"] = "تم إنشاء الشركة بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }

            await PopulateAvailablePlans(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(id);
            if (tenant == null) return NotFound();

            var model = new TenantEditVM
            {
                Id = tenant.Id,
                Name = tenant.Name,
                CommercialRegNo = tenant.CommercialRegNo,
                Phone = tenant.Phone,
                PlanId = tenant.PlanId,
                ContractEndDate = tenant.ContractEndDate,
                NitaqatColor = tenant.NitaqatColor,
                Status = tenant.Status,
                LogoPath = tenant.LogoPath
            };

            await PopulateAvailablePlans(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TenantEditVM model)
        {
            if (ModelState.IsValid)
            {
                var tenant = await _uow.Tenants.GetByIdAsync(model.Id);
                if (tenant == null) return NotFound();

                // Check CommercialRegNo uniqueness
                var existing = await _uow.Tenants.FindAsync(t => t.CommercialRegNo == model.CommercialRegNo && t.Id != model.Id);
                if (existing.Any())
                {
                    ModelState.AddModelError("CommercialRegNo", "رقم السجل التجاري مسجل مسبقاً");
                }
                else
                {
                    tenant.Name = model.Name;
                    tenant.CommercialRegNo = model.CommercialRegNo;
                    tenant.Phone = model.Phone;
                    tenant.PlanId = model.PlanId;
                    tenant.ContractEndDate = model.ContractEndDate.Value;
                    tenant.NitaqatColor = model.NitaqatColor;
                    tenant.Status = model.Status;
                    
                    tenant.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
                    tenant.UpdatedAt = DateTime.UtcNow;

                    _uow.Tenants.Update(tenant);
                    await _uow.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث بيانات الشركة";
                    return RedirectToAction(nameof(Index));
                }
            }

            await PopulateAvailablePlans(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Freeze(Guid id)
        {
            var tenant = await _uow.Tenants.GetByIdAsync(id);
            if (tenant == null) return NotFound();

            tenant.Status = TenantStatus.Frozen;
            tenant.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            tenant.UpdatedAt = DateTime.UtcNow;

            _uow.Tenants.Update(tenant);
            await _uow.SaveChangesAsync();
            TempData["Success"] = "تم تجميد الشركة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfreeze(Guid id)
        {
            var tenant = await _uow.Tenants.GetByIdAsync(id);
            if (tenant == null) return NotFound();

            tenant.Status = TenantStatus.Active;
            tenant.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            tenant.UpdatedAt = DateTime.UtcNow;

            _uow.Tenants.Update(tenant);
            await _uow.SaveChangesAsync();
            TempData["Success"] = "تم تفعيل الشركة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(Guid id)
        {
            var tenant = await _uow.Tenants.GetByIdAsync(id);
            if (tenant == null) return NotFound();

            tenant.Status = TenantStatus.Suspended;
            tenant.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            tenant.UpdatedAt = DateTime.UtcNow;

            _uow.Tenants.Update(tenant);
            await _uow.SaveChangesAsync();
            TempData["Success"] = "تم إيقاف الشركة";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(id);
            if (tenant == null) return NotFound();

            var employees = await _uow.Employees.FindAsync(e => e.TenantId == id);
            
            // Reusing TenantEditVM for Details as requested, or create a specific DetailsVM
            var model = new TenantEditVM
            {
                Id = tenant.Id,
                Name = tenant.Name,
                CommercialRegNo = tenant.CommercialRegNo,
                Phone = tenant.Phone,
                PlanId = tenant.PlanId,
                ContractEndDate = tenant.ContractEndDate,
                NitaqatColor = tenant.NitaqatColor,
                Status = tenant.Status,
                LogoPath = tenant.LogoPath
            };

            ViewBag.PlanName = tenant.Plan?.Name;
            ViewBag.EmployeeCount = employees.Count();

            return View(model);
        }

        private async Task PopulateAvailablePlans(dynamic model)
        {
            var plans = await _uow.Plans.GetActivePlansAsync();
            model.AvailablePlans = plans.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} (حتى {p.MaxEmployees} موظف)"
            });
        }
    }
}

using System.Text.Json;
using Intilaqah.Models;
using Intilaqah.Models.ViewModels.SuperAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class PlansController : Controller
    {
        private readonly IUnitOfWork _uow;

        public PlansController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var plans = await _uow.Plans.GetAllAsync();
            var tenants = await _uow.Tenants.GetAllAsync();

            var vm = plans.Select(plan => {
                var features = new Dictionary<string, bool>();
                if (!string.IsNullOrEmpty(plan.FeaturesJson) && plan.FeaturesJson.StartsWith("{"))
                {
                    try
                    {
                        features = JsonSerializer.Deserialize<Dictionary<string, bool>>(plan.FeaturesJson) ?? new Dictionary<string, bool>();
                    }
                    catch
                    {
                        // Ignore parse errors, fallback to empty dict
                    }
                }

                return new PlanListItemVM
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    MaxUsers = plan.MaxUsers,
                    MaxEmployees = plan.MaxEmployees,
                    IsActive = plan.IsActive,
                    TenantsCount = tenants.Count(t => t.PlanId == plan.Id),
                    Features = features
                };
            }).ToList();

            return View(vm);
        }

        public IActionResult Create()
        {
            return View(new PlanCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var features = new Dictionary<string, bool>
                {
                    ["payroll"] = model.FeaturePayroll,
                    ["nitaqat"] = model.FeatureNitaqat,
                    ["alerts"] = model.FeatureAlerts
                };
                var featuresJson = JsonSerializer.Serialize(features);

                var plan = new Plan
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.Empty, // Platform-wide
                    CreatedBy = User.FindFirst("FullName")?.Value ?? "system",
                    CreatedAt = DateTime.UtcNow,
                    Name = model.Name,
                    MaxUsers = model.MaxUsers,
                    MaxEmployees = model.MaxEmployees,
                    IsActive = model.IsActive,
                    FeaturesJson = featuresJson
                };

                await _uow.Plans.AddAsync(plan);
                await _uow.SaveChangesAsync();
                TempData["Success"] = "تم إنشاء الباقة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var plan = await _uow.Plans.GetByIdAsync(id);
            if (plan == null) return NotFound();

            var features = new Dictionary<string, bool>();
            if (!string.IsNullOrEmpty(plan.FeaturesJson) && plan.FeaturesJson.StartsWith("{"))
            {
                try
                {
                    features = JsonSerializer.Deserialize<Dictionary<string, bool>>(plan.FeaturesJson) ?? new Dictionary<string, bool>();
                }
                catch { }
            }

            var model = new PlanCreateVM
            {
                Id = plan.Id,
                Name = plan.Name,
                MaxUsers = plan.MaxUsers,
                MaxEmployees = plan.MaxEmployees,
                IsActive = plan.IsActive,
                FeaturePayroll = features.ContainsKey("payroll") && features["payroll"],
                FeatureNitaqat = features.ContainsKey("nitaqat") && features["nitaqat"],
                FeatureAlerts = features.ContainsKey("alerts") && features["alerts"]
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlanCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var plan = await _uow.Plans.GetByIdAsync(model.Id);
                if (plan == null) return NotFound();

                var features = new Dictionary<string, bool>
                {
                    ["payroll"] = model.FeaturePayroll,
                    ["nitaqat"] = model.FeatureNitaqat,
                    ["alerts"] = model.FeatureAlerts
                };
                
                plan.Name = model.Name;
                plan.MaxUsers = model.MaxUsers;
                plan.MaxEmployees = model.MaxEmployees;
                plan.IsActive = model.IsActive;
                plan.FeaturesJson = JsonSerializer.Serialize(features);
                plan.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
                plan.UpdatedAt = DateTime.UtcNow;

                _uow.Plans.Update(plan);
                await _uow.SaveChangesAsync();
                
                TempData["Success"] = "تم تحديث الباقة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var plan = await _uow.Plans.GetByIdAsync(id);
            if (plan == null) return NotFound();

            plan.IsActive = !plan.IsActive;
            plan.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            plan.UpdatedAt = DateTime.UtcNow;

            _uow.Plans.Update(plan);
            await _uow.SaveChangesAsync();

            TempData["Success"] = plan.IsActive ? "تم تفعيل الباقة" : "تم تعطيل الباقة";
            return RedirectToAction(nameof(Index));
        }
    }
}

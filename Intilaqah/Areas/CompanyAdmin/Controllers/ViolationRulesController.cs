using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class ViolationRulesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ViolationRulesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var rules = (await _uow.ViolationRules.GetAllAsync())
                .OrderBy(r => r.TenantId == Guid.Empty ? 0 : 1) // Global rules first
                .ThenBy(r => r.RuleNumber)
                .ToList();

            return View(rules);
        }

        public IActionResult Create()
        {
            return View(new ViolationRuleVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViolationRuleVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Fetch current tenant ID via a user claim, but we can just let EF Core set it automatically 
            // since the tenant resolver handles it, or we explicitly set it if needed. 
            // However, our BaseEntity setup and TenantResolver will assign TenantId automatically on SaveChanges if not set, 
            // but for safe measure we rely on the framework to inject TenantId or we just create it.
            // Actually, TenantId is required, but DbContext handles it automatically for added entities if configured.
            // Let's assume DbContext doesn't auto-set it for adding unless explicitly done, wait, DbContext doesn't auto-set TenantId in SaveChangesAsync, it only sets audit fields.
            // Let's get TenantId from claims.
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                TempData["Error"] = "حدث خطأ في جلب بيانات الشركة";
                return RedirectToAction(nameof(Index));
            }

            var rule = new ViolationRule
            {
                RuleNumber = model.RuleNumber,
                Title = model.Title,
                Description = model.Description,
                Severity = model.Severity,
                DeductionAmount = model.DeductionAmount,
                IsActive = model.IsActive,
                TenantId = tenantId // Company specific rule
            };

            await _uow.ViolationRules.AddAsync(rule);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم إضافة المخالفة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var rule = await _uow.ViolationRules.GetByIdAsync(id);
            if (rule == null) return NotFound();

            if (rule.TenantId == Guid.Empty)
            {
                TempData["Error"] = "لا يمكن تعديل قواعد النظام العامة";
                return RedirectToAction(nameof(Index));
            }

            var model = new ViolationRuleVM
            {
                RuleNumber = rule.RuleNumber,
                Title = rule.Title,
                Description = rule.Description,
                Severity = rule.Severity,
                DeductionAmount = rule.DeductionAmount,
                IsActive = rule.IsActive
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ViolationRuleVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var rule = await _uow.ViolationRules.GetByIdAsync(id);
            if (rule == null) return NotFound();

            if (rule.TenantId == Guid.Empty)
            {
                TempData["Error"] = "لا يمكن تعديل قواعد النظام العامة";
                return RedirectToAction(nameof(Index));
            }

            rule.RuleNumber = model.RuleNumber;
            rule.Title = model.Title;
            rule.Description = model.Description;
            rule.Severity = model.Severity;
            rule.DeductionAmount = model.DeductionAmount;
            rule.IsActive = model.IsActive;

            _uow.ViolationRules.Update(rule);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث المخالفة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var rule = await _uow.ViolationRules.GetByIdAsync(id);
            if (rule == null)
            {
                TempData["Error"] = "المخالفة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            if (rule.TenantId == Guid.Empty)
            {
                TempData["Error"] = "لا يمكن حذف قواعد النظام العامة";
                return RedirectToAction(nameof(Index));
            }

            // Check if there are violation records using this rule
            var records = await _uow.ViolationRecords.FindAsync(r => r.ViolationRuleId == id);
            if (records.Any())
            {
                TempData["Error"] = "لا يمكن حذف هذه المخالفة لارتباطها بسجلات سابقة للموظفين. يمكنك تعطيلها بدلاً من ذلك.";
                return RedirectToAction(nameof(Index));
            }

            _uow.ViolationRules.Delete(rule);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم حذف المخالفة بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}

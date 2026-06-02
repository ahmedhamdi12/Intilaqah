using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class ViolationsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ViolationsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var violations = (await _uow.ViolationRecords.GetAllAsync())
                .OrderByDescending(v => v.ViolationDate)
                .ToList();

            var employees = (await _uow.Employees.GetAllAsync())
                .ToDictionary(e => e.Id, e => e.FullNameAr);

            var rules = (await _uow.ViolationRules.GetAllAsync())
                .ToDictionary(r => r.Id, r => r.Title);

            ViewBag.Employees = employees;
            ViewBag.Rules = rules;

            return View(violations);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ViolationCreateVM();
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViolationCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var record = new ViolationRecord
            {
                EmployeeId = model.EmployeeId,
                ViolationRuleId = model.ViolationRuleId,
                ViolationDate = model.ViolationDate,
                Notes = model.Notes,
                CreatedBy = User.FindFirst("FullName")?.Value ?? "system"
            };

            await _uow.ViolationRecords.AddAsync(record);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل المخالفة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _uow.ViolationRecords.GetByIdAsync(id);
            if (record == null)
            {
                TempData["Error"] = "المخالفة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            if (record.PayrollRunId != null)
            {
                TempData["Error"] = "لا يمكن حذف مخالفة تم احتسابها في مسير رواتب معتمد";
                return RedirectToAction(nameof(Index));
            }

            _uow.ViolationRecords.Delete(record);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم حذف المخالفة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(ViolationCreateVM model)
        {
            var employees = await _uow.Employees.FindAsync(e => e.IsActive);
            model.Employees = employees.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.FullNameAr} — {e.EmployeeCode}"
            });

            var rules = await _uow.ViolationRules.GetActiveAsync();
            model.Rules = rules.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Title
            });
        }
    }
}

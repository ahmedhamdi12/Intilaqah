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
    public class AdvancesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public AdvancesController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index()
        {
            var advances = (await _uow.SalaryAdvances.GetAllAsync())
                .OrderByDescending(a => a.RequestDate).ToList();
                
            var employees = (await _uow.Employees.GetAllAsync())
                .ToDictionary(e => e.Id, e => e.FullNameAr);

            ViewBag.Employees = employees;
            return View(advances);
        }

        public async Task<IActionResult> Create()
        {
            var employees = await _uow.Employees.FindAsync(e => e.IsActive);
            return View(new SalaryAdvanceCreateVM
            {
                Employees = employees.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text  = $"{e.FullNameAr} — {e.EmployeeCode}"
                })
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalaryAdvanceCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                var emps = await _uow.Employees.FindAsync(e => e.IsActive);
                model.Employees = emps.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text  = $"{e.FullNameAr} — {e.EmployeeCode}"
                });
                return View(model);
            }

            if (model.MonthlyDeduction > model.TotalAmount)
            {
                ModelState.AddModelError("MonthlyDeduction", "القسط الشهري لا يمكن أن يتجاوز إجمالي السلفة");
                var emps = await _uow.Employees.FindAsync(e => e.IsActive);
                model.Employees = emps.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text  = $"{e.FullNameAr} — {e.EmployeeCode}"
                });
                return View(model);
            }

            var advance = new SalaryAdvance
            {
                EmployeeId       = model.EmployeeId,
                TotalAmount      = model.TotalAmount,
                RemainingAmount  = model.TotalAmount,
                MonthlyDeduction = model.MonthlyDeduction,
                Status           = AdvanceStatus.Approved,
                Notes            = model.Notes,
                RequestDate      = DateTime.UtcNow,
                CreatedBy        = User.FindFirst("FullName")?.Value ?? "system",
            };

            await _uow.SalaryAdvances.AddAsync(advance);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل السلفة بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}

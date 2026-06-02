using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.Services.Payroll;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class PayrollController : Controller
    {
        private readonly IUnitOfWork                 _uow;
        private readonly IPayrollService             _payrollService;
        private readonly IWpsExportService           _wpsExportService;
        private readonly IPayrollReportExportService _reportExportService;

        public PayrollController(
            IUnitOfWork uow, 
            IPayrollService payrollService,
            IWpsExportService wpsExportService,
            IPayrollReportExportService reportExportService)
        {
            _uow                 = uow;
            _payrollService      = payrollService;
            _wpsExportService    = wpsExportService;
            _reportExportService = reportExportService;
        }

        public async Task<IActionResult> Index()
        {
            var runs = (await _uow.Payroll.GetAllAsync())
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToList();

            return View(runs);
        }

        public IActionResult Create()
            => View(new PayrollCreateVM());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollCreateVM model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var createdBy = User.FindFirst("FullName")?.Value ?? "system";
                var run = await _payrollService.CreateRunAsync(
                    model.Month, model.Year, model.WorkingDays, createdBy);

                TempData["Success"] = $"تم إنشاء مسير رواتب {GetMonthName(model.Month)} {model.Year}";
                return RedirectToAction("Details", new { id = run.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var run = await _uow.Payroll.GetWithPaySlipsAsync(id);
            if (run == null) return NotFound();

            ViewBag.MonthName = GetMonthName(run.Month);
            return View(run);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var approvedBy = User.FindFirst("FullName")?.Value ?? "system";
                await _payrollService.ApproveRunAsync(id, approvedBy);
                TempData["Success"] = "تم اعتماد مسير الرواتب بنجاح";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> ExportWps(Guid id)
        {
            try
            {
                var exportedBy = User.FindFirst("FullName")?.Value ?? "system";
                var bytes = await _wpsExportService.ExportWpsExcelAsync(id, exportedBy);
                var run   = await _uow.Payroll.GetByIdAsync(id);
                var fileName = $"WPS_{run?.Month}_{run?.Year}.xlsx";

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"خطأ في التصدير: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        public async Task<IActionResult> ExportInternalReport(Guid id)
        {
            try
            {
                var bytes = await _reportExportService.ExportInternalReportAsync(id);
                var run   = await _uow.Payroll.GetByIdAsync(id);
                var fileName = $"Payroll_Internal_{run?.Month}_{run?.Year}.xlsx";

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"خطأ في التصدير: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        public async Task<IActionResult> Payslip(Guid id)
        {
            var paySlips = await _uow.Payroll.GetPaySlipsByRunAsync(id);
            var run      = await _uow.Payroll.GetByIdAsync(id);
            if (run == null) return NotFound();

            ViewBag.Run       = run;
            ViewBag.MonthName = GetMonthName(run.Month);
            return View(paySlips);
        }

        private static string GetMonthName(int month) => month switch
        {
            1  => "يناير",  2  => "فبراير", 3  => "مارس",
            4  => "أبريل",  5  => "مايو",   6  => "يونيو",
            7  => "يوليو",  8  => "أغسطس",  9  => "سبتمبر",
            10 => "أكتوبر", 11 => "نوفمبر", 12 => "ديسمبر",
            _  => month.ToString()
        };
    }
}

using Intilaqah.Models;
using Intilaqah.Models.ViewModels.Employee;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intilaqah.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class PayslipsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public PayslipsController(IUnitOfWork uow) => _uow = uow;

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

            var approvedRuns = (await _uow.Payroll.GetAllAsync())
                .Where(r => r.Status == PayrollStatus.Approved
                         || r.Status == PayrollStatus.Exported)
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToList();

            var payslips = new List<PayslipListItemVM>();
            foreach (var run in approvedRuns)
            {
                var slip = await _uow.Payroll
                    .GetPaySlipByEmployeeRunAsync(employee.Id, run.Id);
                if (slip == null) continue;

                payslips.Add(new PayslipListItemVM
                {
                    PayrollRunId   = run.Id,
                    Month          = run.Month,
                    Year           = run.Year,
                    MonthName      = GetMonthName(run.Month) + " " + run.Year,
                    GrossSalary    = slip.GrossSalary,
                    TotalDeductions = slip.TotalDeductions,
                    NetSalary      = slip.NetSalary,
                    PresentDays    = slip.PresentDays,
                    AbsentDays     = slip.AbsentDays,
                });
            }

            ViewBag.Employee = employee;
            return View(payslips);
        }

        public async Task<IActionResult> Details(Guid runId)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            var run = await _uow.Payroll.GetByIdAsync(runId);
            if (run == null
                || (run.Status != PayrollStatus.Approved
                 && run.Status != PayrollStatus.Exported))
                return NotFound();

            var slip = await _uow.Payroll
                .GetPaySlipByEmployeeRunAsync(employee.Id, runId);
            if (slip == null) return NotFound();

            var contract = await _uow.Contracts
                .GetActiveContractAsync(employee.Id);

            ViewBag.Employee = employee;
            ViewBag.Run      = run;
            ViewBag.Contract = contract;
            ViewBag.MonthName = GetMonthName(run.Month) + " " + run.Year;

            return View(slip);
        }

        private static string GetMonthName(int month) => month switch
        {
            1=>"يناير",2=>"فبراير",3=>"مارس",4=>"أبريل",
            5=>"مايو",6=>"يونيو",7=>"يوليو",8=>"أغسطس",
            9=>"سبتمبر",10=>"أكتوبر",11=>"نوفمبر",12=>"ديسمبر",
            _=>month.ToString()
        };
    }
}

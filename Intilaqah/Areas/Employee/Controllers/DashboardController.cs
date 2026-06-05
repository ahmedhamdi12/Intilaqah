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
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var userId   = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employee = (await _uow.Employees
                .FindAsync(e => e.UserId == userId && e.IsActive))
                .FirstOrDefault();

            if (employee == null)
                return RedirectToAction("Login", "Account",
                    new { area = "" });

            var today      = DateTime.Today;
            var contract   = await _uow.Contracts
                .GetActiveContractAsync(employee.Id);
            var departments = (await _uow.Departments.GetAllAsync())
                .ToDictionary(d => d.Id, d => d.Name);

            // Today's attendance
            var todayLog = await _uow.Attendance
                .GetByEmployeeDateAsync(employee.Id, today);

            // Shift
            var shiftAssignment = await _uow.ShiftAssignments
                .GetActiveByEmployeeAsync(employee.Id);

            // Leave balance
            var allLeaves = (await _uow.LeaveRequests
                .GetByEmployeeAsync(employee.Id)).ToList();
            var approvedLeaves = allLeaves
                .Where(l => l.Status == LeaveRequestStatus.Approved).ToList();
            var yearsOfService = (today - employee.HireDate).Days / 365;
            var annualDays     = yearsOfService >= 5 ? 30 : 21;
            var usedDays       = approvedLeaves
                .Where(l => l.LeaveType == LeaveType.Annual
                         && l.StartDate.Year == today.Year)
                .Sum(l => l.DurationDays);

            var remaining = Math.Max(0, annualDays - usedDays);

            // Last payslip
            var allRuns = (await _uow.Payroll.GetAllAsync())
                .Where(r => r.Status == PayrollStatus.Approved
                         || r.Status == PayrollStatus.Exported)
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .FirstOrDefault();

            PaySlip? lastPaySlip = null;
            if (allRuns != null)
            {
                lastPaySlip = await _uow.Payroll
                    .GetPaySlipByEmployeeRunAsync(employee.Id, allRuns.Id);
            }

            // Expiring documents
            var docs = (await _uow.Documents
                .GetByEntityAsync(employee.Id, DocumentEntityType.Employee))
                .Where(d => d.ExpiryDate.HasValue)
                .Select(d => new DocumentAlertItem
                {
                    DocType      = d.DocType,
                    ExpiryDate   = d.ExpiryDate,
                    DaysRemaining = (d.ExpiryDate!.Value.Date - today).Days,
                })
                .Where(d => d.DaysRemaining <= 90)
                .OrderBy(d => d.DaysRemaining)
                .ToList();

            var vm = new EmployeeDashboardVM
            {
                FullNameAr     = employee.FullNameAr,
                JobTitle       = employee.JobTitle,
                EmployeeCode   = employee.EmployeeCode,
                Department     = employee.DepartmentId.HasValue
                    ? departments.GetValueOrDefault(
                        employee.DepartmentId.Value, "—")
                    : "—",

                TodayStatus    = todayLog?.Status,
                TodayCheckIn   = todayLog?.CheckIn,
                TodayCheckOut  = todayLog?.CheckOut,
                ShiftName      = shiftAssignment?.Shift?.Name ?? "—",

                AnnualLeaveDays    = annualDays,
                UsedLeaveDays      = usedDays,
                RemainingLeaveDays = remaining,
                PendingLeaveRequests = allLeaves
                    .Count(l => l.Status == LeaveRequestStatus.Pending),

                LastPayslipMonth  = allRuns != null
                    ? GetMonthName(allRuns.Month) + " " + allRuns.Year
                    : null,
                LastNetSalary     = lastPaySlip?.NetSalary,
                LastPayrollRunId  = allRuns?.Id,

                ExpiringDocumentsCount = docs.Count,
                ExpiringDocuments      = docs,

                ContractEndDate  = contract?.EndDate,
                ContractExpiring = contract?.EndDate.HasValue == true
                    && (contract.EndDate.Value - today).Days <= 30,
            };

            ViewBag.UserFullName = employee.FullNameAr;
            ViewBag.JobTitle     = employee.JobTitle;
            ViewBag.ProfilePicturePath = employee.ProfilePicturePath;

            return View(vm);
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

using Intilaqah.Models;
using Intilaqah.Models.ViewModels.Employee;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Intilaqah.Infrastructure.Audit;

namespace Intilaqah.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class LeavesController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _auditService;

        public LeavesController(IUnitOfWork uow, IAuditService auditService)
        {
            _uow = uow;
            _auditService = auditService;
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

            var requests = await _uow.LeaveRequests
                .GetByEmployeeAsync(employee.Id);

            ViewBag.Employee = employee;
            return View(requests);
        }

        public async Task<IActionResult> Create()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            var today = DateTime.Today;
            var approvedLeaves = (await _uow.LeaveRequests
                .GetByEmployeeAsync(employee.Id))
                .Where(l => l.Status == LeaveRequestStatus.Approved
                         && l.LeaveType == LeaveType.Annual
                         && l.StartDate.Year == today.Year)
                .ToList();

            var yearsOfService = (today - employee.HireDate).Days / 365;
            var annualDays     = yearsOfService >= 5 ? 30 : 21;
            var usedDays       = approvedLeaves.Sum(l => l.DurationDays);

            return View(new LeaveRequestCreateVM
            {
                RemainingAnnualDays = Math.Max(0, annualDays - usedDays),
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequestCreateVM model)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            if (model.StartDate <= DateTime.Today)
            {
                ModelState.AddModelError("StartDate",
                    "تاريخ البداية يجب أن يكون من الغد على الأقل");
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate",
                    "تاريخ النهاية يجب أن يكون بعد تاريخ البداية");
            }

            if (model.StartDate > DateTime.Today
                && model.EndDate >= model.StartDate)
            {
                var hasOverlap = await _uow.LeaveRequests.HasOverlapAsync(
                    employee.Id, model.StartDate, model.EndDate);
                if (hasOverlap)
                {
                    ModelState.AddModelError("",
                        "لديك طلب إجازة آخر يتداخل مع هذه الفترة");
                }
            }

            if (!ModelState.IsValid)
            {
                var today2 = DateTime.Today;
                var approved2 = (await _uow.LeaveRequests
                    .GetByEmployeeAsync(employee.Id))
                    .Where(l => l.Status == LeaveRequestStatus.Approved
                             && l.LeaveType == LeaveType.Annual
                             && l.StartDate.Year == today2.Year)
                    .ToList();
                var yrs  = (today2 - employee.HireDate).Days / 365;
                var ann  = yrs >= 5 ? 30 : 21;
                model.RemainingAnnualDays = Math.Max(0, ann - approved2.Sum(l => l.DurationDays));
                return View(model);
            }

            var duration = CalculateWorkingDays(model.StartDate, model.EndDate);

            var request = new LeaveRequest
            {
                EmployeeId   = employee.Id,
                LeaveType    = model.LeaveType,
                StartDate    = model.StartDate,
                EndDate      = model.EndDate,
                DurationDays = duration,
                Reason       = model.Reason,
                Status       = LeaveRequestStatus.Pending,
            };

            await _uow.LeaveRequests.AddAsync(request);
            await _uow.SaveChangesAsync();
            
            await _auditService.LogAsync("Create", "LeaveRequest", request.Id.ToString(), null, 
                $"LeaveType: {model.LeaveType}, Start: {model.StartDate:yyyy-MM-dd}, End: {model.EndDate:yyyy-MM-dd}");

            TempData["Success"] = "تم تقديم طلب الإجازة بنجاح. في انتظار الموافقة.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Forbid();

            var request = await _uow.LeaveRequests.GetByIdAsync(id);
            if (request == null || request.EmployeeId != employee.Id)
                return NotFound();

            if (request.Status != LeaveRequestStatus.Pending)
            {
                TempData["Error"] = "لا يمكن إلغاء هذا الطلب";
                return RedirectToAction(nameof(Index));
            }

            var oldStatus = request.Status;
            request.Status = LeaveRequestStatus.Cancelled;

            _uow.LeaveRequests.Update(request);
            await _uow.SaveChangesAsync();
            
            await _auditService.LogAsync("Cancel", "LeaveRequest", request.Id.ToString(), 
                $"Status: {oldStatus}", $"Status: {request.Status}");

            TempData["Success"] = "تم إلغاء طلب الإجازة";
            return RedirectToAction(nameof(Index));
        }

        private static int CalculateWorkingDays(DateTime start, DateTime end)
        {
            var days = 0;
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Friday
                 && d.DayOfWeek != DayOfWeek.Saturday)
                    days++;
            }
            return days;
        }
    }
}

using Intilaqah.Models;
using Intilaqah.Data;
using Intilaqah.Infrastructure.Notifications;
using Intilaqah.Infrastructure.Audit;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class LeavesController : Controller
    {
        private readonly IUnitOfWork                  _uow;
        private readonly INotificationService         _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService                _auditService;

        public LeavesController(
            IUnitOfWork uow,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _uow                 = uow;
            _notificationService = notificationService;
            _userManager         = userManager;
            _auditService        = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var pending  = await _uow.LeaveRequests.GetPendingAsync();
            var all      = (await _uow.LeaveRequests.GetAllAsync())
                .OrderByDescending(l => l.CreatedAt).ToList();

            var employees = (await _uow.Employees.GetAllAsync())
                .ToDictionary(e => e.Id, e => e.FullNameAr);

            ViewBag.PendingCount = pending.Count();
            ViewBag.Employees    = employees;
            return View(all);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id, string? notes)
        {
            var request = await _uow.LeaveRequests.GetByIdAsync(id);
            if (request == null) return NotFound();

            if (request.Status != LeaveRequestStatus.Pending)
            {
                TempData["Error"] = "تمت معالجة هذا الطلب مسبقاً.";
                return RedirectToAction(nameof(Index));
            }

            var oldStatus = request.Status;
            request.Status        = LeaveRequestStatus.Approved;
            request.ReviewedBy    = User.FindFirst("FullName")?.Value;
            request.ReviewedAt    = DateTime.UtcNow;
            request.ReviewerNotes = notes;

            _uow.LeaveRequests.Update(request);
            await _uow.SaveChangesAsync();

            await _auditService.LogAsync("Approve", "LeaveRequest", request.Id.ToString(), 
                $"Status: {oldStatus}", $"Status: {request.Status}, Notes: {notes}");

            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId);
            if (employee != null)
            {
                var empUser = await _userManager.FindByIdAsync(employee.UserId);
                if (empUser != null)
                {
                    await _notificationService.SendAsync(
                        empUser.Id,
                        request.TenantId,
                        NotificationType.LeaveRequest,
                        "تمت الموافقة على إجازتك",
                        $"تمت الموافقة على طلب إجازتك من {request.StartDate:dd/MM/yyyy} إلى {request.EndDate:dd/MM/yyyy}",
                        "/Employee/Leaves");
                }
            }

            TempData["Success"] = "تمت الموافقة على طلب الإجازة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string? notes)
        {
            var request = await _uow.LeaveRequests.GetByIdAsync(id);
            if (request == null) return NotFound();

            if (request.Status != LeaveRequestStatus.Pending)
            {
                TempData["Error"] = "تمت معالجة هذا الطلب مسبقاً.";
                return RedirectToAction(nameof(Index));
            }

            var oldStatus = request.Status;
            request.Status        = LeaveRequestStatus.Rejected;
            request.ReviewedBy    = User.FindFirst("FullName")?.Value;
            request.ReviewedAt    = DateTime.UtcNow;
            request.ReviewerNotes = notes;

            _uow.LeaveRequests.Update(request);
            await _uow.SaveChangesAsync();

            await _auditService.LogAsync("Reject", "LeaveRequest", request.Id.ToString(), 
                $"Status: {oldStatus}", $"Status: {request.Status}, Notes: {notes}");

            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId);
            if (employee != null)
            {
                var empUser = await _userManager.FindByIdAsync(employee.UserId);
                if (empUser != null)
                {
                    await _notificationService.SendAsync(
                        empUser.Id,
                        request.TenantId,
                        NotificationType.LeaveRequest,
                        "تم رفض طلب إجازتك",
                        $"تم رفض طلب إجازتك من {request.StartDate:dd/MM/yyyy} إلى {request.EndDate:dd/MM/yyyy}. {notes}",
                        "/Employee/Leaves");
                }
            }

            TempData["Error"] = "تم رفض طلب الإجازة";
            return RedirectToAction(nameof(Index));
        }
    }
}

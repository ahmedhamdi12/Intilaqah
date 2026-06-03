using Intilaqah.Infrastructure.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET /CompanyAdmin/Notifications
        public async Task<IActionResult> Index()
        {
            var userId        = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var notifications = await _notificationService
                .GetRecentAsync(userId, 50);

            ViewBag.UnreadCount = await _notificationService
                .GetUnreadCountAsync(userId);

            return View(notifications);
        }

        // POST /CompanyAdmin/Notifications/MarkRead/id
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        // POST /CompanyAdmin/Notifications/MarkAllRead
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            await _notificationService.MarkAllAsReadAsync(userId);
            TempData["Success"] = "تم تحديد جميع الإشعارات كمقروءة";
            return RedirectToAction(nameof(Index));
        }
    }
}

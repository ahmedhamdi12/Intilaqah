using Intilaqah.Data;
using Intilaqah.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Infrastructure.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext         _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context     = context;
            _userManager = userManager;
        }

        public async Task SendAsync(
            string userId,
            Guid?  tenantId,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId    = userId,
                TenantId  = tenantId,
                Type      = type,
                Title     = title,
                Message   = message,
                ActionUrl = actionUrl,
                IsRead    = false,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task SendToTenantUsersAsync(
            Guid tenantId,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null)
        {
            // Get all users for this tenant
            var users = _userManager.Users
                .Where(u => u.TenantId == tenantId && u.IsActive)
                .ToList();

            foreach (var user in users)
            {
                await SendAsync(
                    user.Id, tenantId, type, title, message, actionUrl);
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _context.Notifications
                // Bypass QueryFilter to query by specific userId
                .IgnoreQueryFilters()
                .CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<IEnumerable<Notification>> GetRecentAsync(
            string userId, int count = 10)
            => await _context.Notifications
                .IgnoreQueryFilters()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var n = await _context.Notifications
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.Id == notificationId);
            if (n == null) return;

            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Notifications
                .IgnoreQueryFilters()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}

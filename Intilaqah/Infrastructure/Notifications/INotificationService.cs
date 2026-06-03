using Intilaqah.Models;

namespace Intilaqah.Infrastructure.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(
            string userId,
            Guid?  tenantId,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null);

        Task SendToTenantUsersAsync(
            Guid tenantId,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null);

        Task<int>  GetUnreadCountAsync(string userId);
        Task<IEnumerable<Notification>> GetRecentAsync(
            string userId, int count = 10);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}

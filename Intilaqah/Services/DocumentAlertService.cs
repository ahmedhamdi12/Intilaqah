using Intilaqah.Data;
using Intilaqah.Infrastructure.Notifications;
using Intilaqah.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Services
{
    public interface IDocumentAlertService
    {
        Task SendExpiryAlertsAsync();
        Task<IEnumerable<Document>> GetCriticalDocumentsAsync();
    }

    public class DocumentAlertService : IDocumentAlertService
    {
        private readonly ApplicationDbContext         _context;
        private readonly INotificationService         _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentAlertService(
            ApplicationDbContext         context,
            INotificationService         notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _context             = context;
            _notificationService = notificationService;
            _userManager         = userManager;
        }

        public async Task SendExpiryAlertsAsync()
        {
            var today      = DateTime.UtcNow.Date;
            var thresholds = new[] { 90, 60, 30 };

            var docs = await _context.Documents
                .IgnoreQueryFilters()
                .Where(d => !d.IsDeleted
                         && d.ExpiryDate.HasValue
                         && d.ExpiryDate.Value.Date >= today
                         && d.ExpiryDate.Value.Date <= today.AddDays(90))
                .ToListAsync();

            var empIds = docs
                .Where(d => d.EntityType == DocumentEntityType.Employee)
                .Select(d => d.EntityId)
                .Distinct().ToList();

            var employees = await _context.Employees
                .IgnoreQueryFilters()
                .Where(e => empIds.Contains(e.Id) && !e.IsDeleted)
                .ToDictionaryAsync(e => e.Id, e => e.FullNameAr);

            var byTenant = docs.GroupBy(d => d.TenantId);

            foreach (var tenantGroup in byTenant)
            {
                var tenantId = tenantGroup.Key;
                var admins   = _userManager.Users
                    .Where(u => u.TenantId == tenantId && u.IsActive)
                    .ToList();

                if (!admins.Any()) continue;

                foreach (var doc in tenantGroup)
                {
                    var daysLeft = (doc.ExpiryDate!.Value.Date - today).Days;
                    if (!thresholds.Contains(daysLeft)) continue;

                    var entityName = doc.EntityType ==
                        DocumentEntityType.Employee
                        && employees.TryGetValue(
                            doc.EntityId, out var empName)
                        ? empName
                        : "الشركة";

                    var title   = $"تنبيه وثيقة: {doc.DocType} — {entityName}";
                    var message = daysLeft <= 0
                        ? $"وثيقة '{doc.DocType}' للـ {entityName} انتهت!"
                        : $"وثيقة '{doc.DocType}' للـ {entityName} تنتهي خلال {daysLeft} يوم";

                    var actionUrl = doc.EntityType ==
                        DocumentEntityType.Employee
                        ? $"/CompanyAdmin/Documents/Employee?employeeId={doc.EntityId}"
                        : "/CompanyAdmin/Documents/Company";

                    foreach (var admin in admins)
                    {
                        await _notificationService.SendAsync(
                            admin.Id, tenantId,
                            NotificationType.DocumentExpiry,
                            title, message, actionUrl);
                    }
                }
            }
        }

        public async Task<IEnumerable<Document>> GetCriticalDocumentsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var cutoff = today.AddDays(30);
            return await _context.Documents
                .IgnoreQueryFilters()
                .Where(d => !d.IsDeleted
                         && d.ExpiryDate.HasValue
                         && d.ExpiryDate.Value.Date >= today
                         && d.ExpiryDate.Value.Date <= cutoff)
                .OrderBy(d => d.ExpiryDate)
                .ToListAsync();
        }
    }
}

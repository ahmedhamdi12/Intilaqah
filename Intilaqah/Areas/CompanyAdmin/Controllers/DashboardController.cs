using Intilaqah.UnitOfWork;
using Intilaqah.Infrastructure.Notifications;
using Intilaqah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;
        private readonly INitaqatService _nitaqatService;

        public DashboardController(
            IUnitOfWork uow,
            INotificationService notificationService,
            INitaqatService nitaqatService)
        {
            _uow                 = uow;
            _notificationService = notificationService;
            _nitaqatService      = nitaqatService;
        }

        public async Task<IActionResult> Index()
        {
            var nitaqat = await _nitaqatService.GetCurrentZoneAsync();
            ViewBag.SaudizationPct  = nitaqat.SaudizationPercentage;
            ViewBag.SaudiCount      = nitaqat.SaudiCount;
            ViewBag.TotalEmployees  = nitaqat.TotalCount;
            ViewBag.NitaqatColor    = nitaqat.CssClass;
            ViewBag.NitaqatLabel    = nitaqat.ZoneDetail;
            ViewBag.NitaqatNeeded   = nitaqat.NeededForNextZone;
            ViewBag.NitaqatNextZone = nitaqat.NextZoneLabel;
            
            var expiringDocs = await _uow.Documents.GetExpiringAsync(30);
            var expiringDocsCount = expiringDocs.Count();
            ViewBag.ExpiringDocs = expiringDocsCount;
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            ViewBag.NotifCount = await _notificationService.GetUnreadCountAsync(userId);
            
            ViewBag.UserFullName = User.FindFirst("FullName")?.Value;

            var tenantIdStr = User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdStr, out var tenantId))
            {
                var tenant = await _uow.Tenants.GetByIdWithPlanAsync(tenantId);
                ViewBag.TenantName = tenant?.Name ?? "المنشأة";
            }
            else
            {
                ViewBag.TenantName = "المنشأة";
            }

            return View();
        }
    }
}

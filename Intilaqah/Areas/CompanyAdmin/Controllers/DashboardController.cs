using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var allEmployees = await _uow.Employees.GetAllAsync();
            ViewBag.TotalEmployees = allEmployees.Count(e => e.IsActive);
            ViewBag.SaudiCount = await _uow.Employees.CountSaudiAsync();
            ViewBag.SaudizationPct = await _uow.Employees.GetSaudizationPercentageAsync();
            
            var expiringDocs = await _uow.Documents.GetExpiringAsync(30);
            var expiringDocsCount = expiringDocs.Count();
            ViewBag.ExpiringDocs = expiringDocsCount;
            ViewBag.NotifCount = expiringDocsCount;
            ViewBag.UserFullName = User.FindFirst("FullName")?.Value;

            decimal saudizationPct = ViewBag.SaudizationPct;
            if (saudizationPct >= 40)
            {
                ViewBag.NitaqatColor = "platinum";
                ViewBag.NitaqatLabel = "بلاتيني";
            }
            else if (saudizationPct >= 30)
            {
                ViewBag.NitaqatColor = "green";
                ViewBag.NitaqatLabel = "أخضر";
            }
            else if (saudizationPct >= 20)
            {
                ViewBag.NitaqatColor = "yellow";
                ViewBag.NitaqatLabel = "أصفر";
            }
            else
            {
                ViewBag.NitaqatColor = "red";
                ViewBag.NitaqatLabel = "أحمر";
            }

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

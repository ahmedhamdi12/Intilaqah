using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveTenantsCount = await _uow.Tenants.CountActiveAsync();
            ViewBag.FrozenTenantsCount = await _uow.Tenants.CountFrozenAsync();
            var expiringContracts = await _uow.Tenants.GetExpiringContractsAsync(30);
            ViewBag.ExpiringContractsCount = expiringContracts.Count();
            ViewBag.UserFullName = User.FindFirst("FullName")?.Value;

            var expiringTenants = expiringContracts.Take(5).ToList();
            ViewBag.ExpiringTenants = expiringTenants;

            var latestTenants = (await _uow.Tenants.GetAllAsync())
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToList();
            ViewBag.LatestTenants = latestTenants;

            // Optional: get actual plans count
            var plans = await _uow.Plans.GetAllAsync();
            ViewBag.PlansCount = plans.Count();

            return View();
        }
    }
}

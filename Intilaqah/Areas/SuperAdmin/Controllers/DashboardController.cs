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
            ViewBag.ExpiringContracts = expiringContracts.Count();
            ViewBag.UserFullName = User.FindFirst("FullName")?.Value;

            return View();
        }
    }
}

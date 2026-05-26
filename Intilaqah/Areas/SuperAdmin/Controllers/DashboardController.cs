using Intilaqah.Models;
using Intilaqah.Models.ViewModels.SuperAdmin;
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
            var allTenants = (await _uow.Tenants.GetAllAsync()).ToList();
            var allPlans   = (await _uow.Plans.GetAllAsync()).ToList();
            var empCounts  = await _uow.Tenants.GetEmployeeCountsPerTenantAsync();
            var planLookup = allPlans.ToDictionary(p => p.Id, p => p.Name);
            var expiring30 = (await _uow.Tenants.GetExpiringContractsAsync(30)).ToList();
            var expiring7  = (await _uow.Tenants.GetExpiringContractsAsync(7)).ToList();
            var today      = DateTime.UtcNow.Date;

            var vm = new SuperAdminDashboardVM
            {
                TotalCompanies     = allTenants.Count,
                ActiveCompanies    = allTenants.Count(t => t.Status == TenantStatus.Active),
                FrozenCompanies    = allTenants.Count(t => t.Status == TenantStatus.Frozen),
                SuspendedCompanies = allTenants.Count(t => t.Status == TenantStatus.Suspended),
                TotalEmployees     = empCounts.Values.Sum(),
                ExpiringContracts30= expiring30.Count,
                ExpiringContracts7 = expiring7.Count,
                TotalPlans         = allPlans.Count,

                NitaqatPlatinum = allTenants.Count(t => t.NitaqatColor == NitaqatColor.Platinum),
                NitaqatGreen    = allTenants.Count(t => t.NitaqatColor == NitaqatColor.Green),
                NitaqatYellow   = allTenants.Count(t => t.NitaqatColor == NitaqatColor.Yellow),
                NitaqatRed      = allTenants.Count(t => t.NitaqatColor == NitaqatColor.Red),

                ExpiringContracts = expiring30.Select(t => new ExpiringContractVM {
                    Id              = t.Id,
                    CompanyName     = t.Name,
                    PlanName        = planLookup.GetValueOrDefault(t.PlanId, "—"),
                    ContractEndDate = t.ContractEndDate,
                    DaysRemaining   = (t.ContractEndDate.Date - today).Days,
                    UrgencyClass    = (t.ContractEndDate.Date - today).Days <= 7  ? "danger"
                                    : (t.ContractEndDate.Date - today).Days <= 30 ? "warning"
                                    : "info"
                }).OrderBy(x => x.DaysRemaining).ToList(),

                RecentCompanies = allTenants
                    .OrderByDescending(t => t.CreatedAt).Take(6)
                    .Select(t => new RecentCompanyVM {
                        Id            = t.Id,
                        Name          = t.Name,
                        PlanName      = planLookup.GetValueOrDefault(t.PlanId, "—"),
                        Status        = t.Status,
                        NitaqatColor  = t.NitaqatColor,
                        CreatedAt     = t.CreatedAt,
                        EmployeeCount = empCounts.GetValueOrDefault(t.Id, 0)
                    }).ToList(),

                PlanUtilizations = allPlans.Select(p => new PlanUtilizationVM {
                    Name         = p.Name,
                    TenantsCount = allTenants.Count(t => t.PlanId == p.Id),
                    MaxEmployees = p.MaxEmployees,
                    IsActive     = p.IsActive
                }).ToList()
            };

            ViewBag.UserFullName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name;
            return View(vm);
        }
    }
}

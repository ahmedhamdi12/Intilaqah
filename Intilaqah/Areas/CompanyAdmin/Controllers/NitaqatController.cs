using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class NitaqatController : Controller
    {
        private readonly IUnitOfWork     _uow;
        private readonly INitaqatService _nitaqatService;

        public NitaqatController(
            IUnitOfWork uow, INitaqatService nitaqatService)
        {
            _uow            = uow;
            _nitaqatService = nitaqatService;
        }

        // GET /CompanyAdmin/Nitaqat
        public async Task<IActionResult> Index()
        {
            var currentZone = await _nitaqatService.GetCurrentZoneAsync();
            var employees   = (await _uow.Employees.GetAllAsync()).ToList();
            var departments = (await _uow.Departments.GetAllAsync())
                .ToDictionary(d => d.Id, d => d.Name);

            // Department breakdown
            var deptBreakdown = employees
                .Where(e => e.IsActive && e.DepartmentId.HasValue)
                .GroupBy(e => e.DepartmentId!.Value)
                .Select(g => {
                    var saudi = g.Count(e =>
                        e.Nationality == NationalityType.Saudi);
                    var total = g.Count();
                    return new DepartmentNitaqatItem
                    {
                        DepartmentName = departments
                            .GetValueOrDefault(g.Key, "—"),
                        SaudiCount  = saudi,
                        TotalCount  = total,
                        Percentage  = total > 0
                            ? Math.Round((decimal)saudi / total * 100, 1)
                            : 0,
                    };
                })
                .OrderByDescending(d => d.Percentage)
                .ToList();

            // Required professions (static list per Saudi labor rules)
            var requiredProfessions = GetRequiredProfessions(employees);

            var vm = new NitaqatPageVM
            {
                CurrentZone          = currentZone,
                DepartmentBreakdown  = deptBreakdown,
                RequiredProfessions  = requiredProfessions,
            };

            return View(vm);
        }

        // POST /CompanyAdmin/Nitaqat/Simulate
        // AJAX endpoint — returns JSON. 
        // This endpoint calculates the expected Nitaqat zone based on hypothetical changes
        // but does NOT persist any changes to the database. It is strictly a calculator for the client-side simulator.
        [HttpPost]
        public async Task<IActionResult> Simulate(
            int addSaudi, int addNonSaudi,
            int removeSaudi, int removeNonSaudi)
        {
            var current = await _nitaqatService.GetCurrentZoneAsync();
            var result  = _nitaqatService.SimulateZone(
                current.SaudiCount, current.TotalCount,
                addSaudi, addNonSaudi,
                removeSaudi, removeNonSaudi);

            return Json(new {
                percentage     = result.SaudizationPercentage,
                zoneDetail     = result.ZoneDetail,
                cssClass       = result.CssClass,
                neededForNext  = result.NeededForNextZone,
                nextZoneLabel  = result.NextZoneLabel,
                saudiCount     = result.SaudiCount,
                totalCount     = result.TotalCount,
            });
        }

        private static List<ProfessionNitaqatItem> GetRequiredProfessions(
            List<Intilaqah.Models.Employee> employees)
        {
            // Saudi labor law mandated saudization % per profession
            var mandated = new[]
            {
                ("محاسب",          75),
                ("مدير موارد بشرية", 100),
                ("أمين صندوق",     95),
                ("موظف استقبال",   100),
                ("مدير مبيعات",    50),
            };

            return mandated.Select(m => {
                var profEmps = employees
                    .Where(e => e.IsActive
                             && e.JobTitle != null
                             && e.JobTitle.Contains(m.Item1,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var saudi = profEmps.Count(e =>
                    e.Nationality == NationalityType.Saudi);
                var total = profEmps.Count;
                var pct   = total > 0
                    ? Math.Round((decimal)saudi / total * 100, 1) : 0;

                return new ProfessionNitaqatItem
                {
                    ProfessionName     = m.Item1,
                    RequiredPercentage = m.Item2,
                    CurrentCount       = saudi,
                    TotalCount         = total,
                    CurrentPercentage  = pct,
                    IsCompliant        = pct >= m.Item2 || total == 0,
                };
            }).ToList();
        }
    }
}

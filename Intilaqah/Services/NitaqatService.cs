using Intilaqah.Models;
using Intilaqah.UnitOfWork;

namespace Intilaqah.Services
{
    public class NitaqatZoneResult
    {
        public decimal      SaudizationPercentage { get; set; }
        public int          SaudiCount            { get; set; }
        public int          NonSaudiCount         { get; set; }
        public int          TotalCount            { get; set; }
        public NitaqatColor Color                 { get; set; }
        public string       ZoneLabel             { get; set; } = "";
        public string       ZoneDetail            { get; set; } = "";
        public decimal      NeededForNextZone     { get; set; }
        public string       NextZoneLabel         { get; set; } = "";
        public string       CssClass              { get; set; } = "";
    }

    public interface INitaqatService
    {
        Task<NitaqatZoneResult> GetCurrentZoneAsync();
        Task UpdateTenantColorAsync(Guid tenantId);
        NitaqatZoneResult SimulateZone(
            int currentSaudi, int currentTotal,
            int addSaudi = 0, int addNonSaudi = 0,
            int removeSaudi = 0, int removeNonSaudi = 0);
    }

    public class NitaqatService : INitaqatService
    {
        private readonly IUnitOfWork _uow;

        public NitaqatService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<NitaqatZoneResult> GetCurrentZoneAsync()
        {
            // EmployeeRepository.CountSaudiAsync already filters by IsActive = true
            var saudi    = await _uow.Employees.CountSaudiAsync();
            var nonSaudi = await _uow.Employees.CountNonSaudiAsync();
            var total    = saudi + nonSaudi;

            return CalculateZone(saudi, total);
        }

        public async Task UpdateTenantColorAsync(Guid tenantId)
        {
            var result = await GetCurrentZoneAsync();
            var tenant = await _uow.Tenants.GetByIdAsync(tenantId);
            if (tenant == null) return;

            tenant.NitaqatColor = result.Color;
            tenant.UpdatedAt    = DateTime.UtcNow;
            _uow.Tenants.Update(tenant);
            await _uow.SaveChangesAsync();
        }

        public NitaqatZoneResult SimulateZone(
            int currentSaudi, int currentTotal,
            int addSaudi = 0, int addNonSaudi = 0,
            int removeSaudi = 0, int removeNonSaudi = 0)
        {
            var newSaudi = Math.Max(0, currentSaudi + addSaudi - removeSaudi);
            var newTotal = Math.Max(0, currentTotal
                + addSaudi + addNonSaudi
                - removeSaudi - removeNonSaudi);
            return CalculateZone(newSaudi, newTotal);
        }

        private static NitaqatZoneResult CalculateZone(int saudi, int total)
        {
            var pct = total == 0 ? 0m : Math.Round((decimal)saudi / total * 100, 2);

            NitaqatColor color;
            string label, detail, cssClass, nextLabel;
            decimal neededForNext;

            if (pct >= 40m)
            {
                color        = NitaqatColor.Platinum;
                label        = "بلاتيني";
                detail       = "بلاتيني";
                cssClass     = "platinum";
                nextLabel    = "—";
                neededForNext = 0;
            }
            else if (pct >= 30m)
            {
                color        = NitaqatColor.Green;
                label        = "أخضر";
                detail       = "أخضر مرتفع";
                cssClass     = "green";
                nextLabel    = "بلاتيني";
                neededForNext = total > 0
                    ? Math.Max(0, (int)Math.Ceiling((0.40m * total - saudi) / (1m - 0.40m)))
                    : 0;
            }
            else if (pct >= 20m)
            {
                color        = NitaqatColor.Green;
                label        = "أخضر";
                detail       = "أخضر منخفض";
                cssClass     = "green";
                nextLabel    = "أخضر مرتفع";
                neededForNext = total > 0
                    ? Math.Max(0, (int)Math.Ceiling((0.30m * total - saudi) / (1m - 0.30m)))
                    : 0;
            }
            else if (pct >= 10m)
            {
                color        = NitaqatColor.Yellow;
                label        = "أصفر";
                detail       = "أصفر";
                cssClass     = "yellow";
                nextLabel    = "أخضر منخفض";
                neededForNext = total > 0
                    ? Math.Max(0, (int)Math.Ceiling((0.20m * total - saudi) / (1m - 0.20m)))
                    : 0;
            }
            else
            {
                color        = NitaqatColor.Red;
                label        = "أحمر";
                detail       = "أحمر";
                cssClass     = "red";
                nextLabel    = "أصفر";
                neededForNext = total > 0
                    ? Math.Max(0, (int)Math.Ceiling((0.10m * total - saudi) / (1m - 0.10m)))
                    : 0;
            }

            return new NitaqatZoneResult
            {
                SaudizationPercentage = pct,
                SaudiCount            = saudi,
                NonSaudiCount         = total - saudi,
                TotalCount            = total,
                Color                 = color,
                ZoneLabel             = label,
                ZoneDetail            = detail,
                CssClass              = cssClass,
                NextZoneLabel         = nextLabel,
                NeededForNextZone     = neededForNext,
            };
        }
    }
}

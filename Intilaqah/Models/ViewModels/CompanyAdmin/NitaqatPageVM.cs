using Intilaqah.Models;
using Intilaqah.Services;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class NitaqatPageVM
    {
        public NitaqatZoneResult CurrentZone { get; set; } = null!;

        // Required professions saudization
        public List<ProfessionNitaqatItem> RequiredProfessions { get; set; } = new();

        // Department breakdown
        public List<DepartmentNitaqatItem> DepartmentBreakdown { get; set; } = new();
    }

    public class ProfessionNitaqatItem
    {
        public string ProfessionName      { get; set; } = "";
        public int    RequiredPercentage  { get; set; }
        public int    CurrentCount        { get; set; }
        public int    TotalCount          { get; set; }
        public decimal CurrentPercentage  { get; set; }
        public bool   IsCompliant         { get; set; }
    }

    public class DepartmentNitaqatItem
    {
        public string  DepartmentName { get; set; } = "";
        public int     SaudiCount     { get; set; }
        public int     TotalCount     { get; set; }
        public decimal Percentage     { get; set; }
    }
}

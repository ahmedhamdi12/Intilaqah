using System.Collections.Generic;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class PlanListItemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MaxUsers { get; set; }
        public int MaxEmployees { get; set; }
        public bool IsActive { get; set; }
        public int TenantsCount { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new Dictionary<string, bool>();
    }
}

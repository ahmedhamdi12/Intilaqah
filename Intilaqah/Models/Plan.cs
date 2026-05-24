using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class Plan : BaseEntity
    {
        
        public string Name { get; set; } = "";
        public int MaxUsers { get; set; }
        public int MaxEmployees { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Tenant> Tenants { get; set; } = [];
    }
}

namespace Intilaqah.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int MaxUsers { get; set; }
        public int MaxEmployees { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Tenant> Tenants { get; set; } = [];
    }
}

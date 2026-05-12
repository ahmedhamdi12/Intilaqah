using System.Reflection.Metadata;

namespace Intilaqah.Models
{
    public enum TenantStatus { Active, Frozen, Suspended }
    public enum NitaqatColor { Platinum, Green, Yellow, Red }
    public class Tenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string CommercialRegNo { get; set; } = "";
        public string? Phone { get; set; }
        public string? LogoPath { get; set; }
        public int PlanId { get; set; }
        public TenantStatus Status { get; set; } = TenantStatus.Active;
        public NitaqatColor NitaqatColor { get; set; }
        public DateTime ContractEndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Plan Plan { get; set; } = null!;
        public ICollection<Employee> Employees { get; set; } = [];
        public ICollection<Document> Documents { get; set; } = [];
    }
}

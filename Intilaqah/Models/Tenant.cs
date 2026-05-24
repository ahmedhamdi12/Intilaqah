using System.Reflection.Metadata;
using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum TenantStatus { Active, Frozen, Suspended }
    public enum NitaqatColor { Platinum, Green, Yellow, Red }
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = "";
        public string CommercialRegNo { get; set; } = "";
        public string? Phone { get; set; }
        public string? LogoPath { get; set; }
        public Guid PlanId { get; set; }
        public TenantStatus Status { get; set; } = TenantStatus.Active;
        public NitaqatColor NitaqatColor { get; set; }
        public DateTime ContractEndDate { get; set; }

        public Plan Plan { get; set; } = null!;
        public ICollection<Employee> Employees { get; set; } = [];
        public ICollection<Document> Documents { get; set; } = [];
    }
}

using System.Reflection.Metadata;

namespace Intilaqah.Models
{
    public enum EmploymentType { FullTime, PartTime, Contract }
    public enum Nationality { Saudi, NonSaudi }
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }    // ← tenant isolation
        public string UserId { get; set; } = ""; // ← Identity user FK
        public string FullNameAr { get; set; } = "";
        public string FullNameEn { get; set; } = "";
        public string NationalId { get; set; } = "";
        public Nationality Nationality { get; set; }
        public string JobTitle { get; set; } = "";
        public DateTime HireDate { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public decimal BasicSalary { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Document> Documents { get; set; } = [];
    }
}

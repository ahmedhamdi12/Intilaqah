using System.Reflection.Metadata;
using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum EmploymentType { FullTime, PartTime, Contract }
    public enum NationalityType { Saudi, NonSaudi }
    public class Employee : BaseEntity
    {
        public string UserId { get; set; } = "";
        public string FullNameAr { get; set; } = "";
        public string FullNameEn { get; set; } = "";
        public string NationalId { get; set; } = "";
        public NationalityType Nationality { get; set; }
        public string JobTitle { get; set; } = "";
        public DateTime HireDate { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public decimal BasicSalary { get; set; }
        public bool IsActive { get; set; } = true;

        public Tenant Tenant { get; set; } = null!;
        public ICollection<Document> Documents { get; set; } = [];
    }
}

using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum ContractType { Unlimited, FixedTerm, Seasonal, Probation }

    public class Contract : BaseEntity
    {
        public Guid           EmployeeId   { get; set; }
        public ContractType   ContractType { get; set; }
        public DateTime       StartDate    { get; set; }
        public DateTime?      EndDate      { get; set; }
        public decimal        BasicSalary  { get; set; }
        public decimal        HousingAllowance  { get; set; }
        public decimal        TransportAllowance { get; set; }
        public decimal        OtherAllowances   { get; set; }
        public bool           IsActive     { get; set; } = true;
        public string?        FilePath     { get; set; }

        // Navigation
        public Employee Employee { get; set; } = null!;
    }
}

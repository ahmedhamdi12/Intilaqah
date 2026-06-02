using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class EmployeeBankAccount : BaseEntity
    {
        public Guid    EmployeeId { get; set; }
        public string  BankName   { get; set; } = "";
        public string  Iban       { get; set; } = "";
        public bool    IsActive   { get; set; } = true;

        // Navigation
        public Employee Employee { get; set; } = null!;
    }
}

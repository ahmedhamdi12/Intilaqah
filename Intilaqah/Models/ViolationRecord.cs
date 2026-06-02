using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class ViolationRecord : BaseEntity
    {
        public Guid     EmployeeId      { get; set; }
        public Guid     ViolationRuleId { get; set; }
        public DateTime ViolationDate   { get; set; }
        public string?  Notes           { get; set; }
        
        public Guid?    PayrollRunId    { get; set; }

        // Navigation
        public Employee      Employee      { get; set; } = null!;
        public ViolationRule ViolationRule { get; set; } = null!;
        public PayrollRun?   PayrollRun    { get; set; }
    }
}

using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class SalaryAdvanceTransaction : BaseEntity
    {
        public Guid     SalaryAdvanceId { get; set; }
        public Guid     PayrollRunId    { get; set; }
        public decimal  Amount          { get; set; }
        public DateTime TransactionDate { get; set; }

        // Navigation
        public SalaryAdvance SalaryAdvance { get; set; } = null!;
        public PayrollRun    PayrollRun    { get; set; } = null!;
    }
}

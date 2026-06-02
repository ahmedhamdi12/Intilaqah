using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum PayrollStatus { Draft, UnderReview, Approved, Exported }

    public class PayrollRun : BaseEntity
    {
        public int           Month        { get; set; }
        public int           Year         { get; set; }
        public int           WorkingDays  { get; set; } = 26;
        public PayrollStatus Status       { get; set; } = PayrollStatus.Draft;
        
        public DateTime?     ApprovedAt   { get; set; }
        public string?       ApprovedBy   { get; set; }
        public string?       Notes        { get; set; }
        
        public decimal       TotalGross   { get; set; }
        public decimal       TotalDeductions { get; set; }
        public decimal       TotalNet     { get; set; }

        // Export Tracking
        public DateTime?     ExportedAt         { get; set; }
        public string?       ExportedBy         { get; set; }
        public string?       WpsExportReference { get; set; }

        public ICollection<PaySlip>                  PaySlips     { get; set; } = [];
        public ICollection<ViolationRecord>          Violations   { get; set; } = [];
        public ICollection<SalaryAdvanceTransaction> AdvanceTransactions { get; set; } = [];
    }
}

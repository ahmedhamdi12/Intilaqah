using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum AdvanceStatus { Pending, Approved, Rejected, FullyDeducted }

    public class SalaryAdvance : BaseEntity
    {
        public Guid          EmployeeId     { get; set; }
        public decimal       TotalAmount    { get; set; }
        public decimal       RemainingAmount { get; set; }
        public decimal       MonthlyDeduction { get; set; }
        public AdvanceStatus Status         { get; set; } = AdvanceStatus.Pending;
        public DateTime      RequestDate    { get; set; } = DateTime.UtcNow;
        public string?       Notes          { get; set; }

        public ICollection<SalaryAdvanceTransaction> Transactions { get; set; } = [];

        // Navigation
        public Employee Employee { get; set; } = null!;
    }
}

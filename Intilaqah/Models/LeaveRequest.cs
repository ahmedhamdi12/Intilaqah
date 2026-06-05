using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum LeaveType
    {
        Annual,       // سنوية
        Sick,         // مرضية
        Emergency,    // طارئة
        Unpaid,       // بدون راتب
        Maternity,    // أمومة
        Hajj,         // حج
    }

    public enum LeaveRequestStatus
    {
        Pending,    // قيد المراجعة
        Approved,   // موافق عليها
        Rejected,   // مرفوضة
        Cancelled,  // ملغاة
    }

    public class LeaveRequest : BaseEntity
    {
        public Guid               EmployeeId     { get; set; }
        public LeaveType          LeaveType      { get; set; }
        public DateTime           StartDate      { get; set; }
        public DateTime           EndDate        { get; set; }
        public int                DurationDays   { get; set; }
        public string?            Reason         { get; set; }
        public LeaveRequestStatus Status         { get; set; } = LeaveRequestStatus.Pending;
        public string?            ReviewedBy     { get; set; }
        public DateTime?          ReviewedAt     { get; set; }
        public string?            ReviewerNotes  { get; set; }

        // Navigation
        public Employee Employee { get; set; } = null!;
    }
}

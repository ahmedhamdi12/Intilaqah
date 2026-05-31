using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum AttendanceStatus
    {
        Present,      // حاضر
        Late,         // متأخر
        Absent,       // غائب
        HalfDay,      // نصف يوم
        OnLeave,      // إجازة
    }

    public class AttendanceLog : BaseEntity
    {
        public Guid             EmployeeId       { get; set; }
        public DateTime         Date             { get; set; }
        public TimeOnly?        CheckIn          { get; set; }
        public TimeOnly?        CheckOut         { get; set; }
        public int              LateMinutes      { get; set; }
        public int              OvertimeMinutes  { get; set; }
        public AttendanceStatus Status           { get; set; }
        public string?          Notes            { get; set; }
        public string?          ImportSource     { get; set; }

        // Navigation
        public Employee Employee { get; set; } = null!;
    }
}

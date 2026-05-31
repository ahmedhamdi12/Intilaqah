using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class AttendanceDashboardVM
    {
        public DateTime Date             { get; set; } = DateTime.Today;
        public int      TotalEmployees   { get; set; }
        public int      PresentCount     { get; set; }
        public int      LateCount        { get; set; }
        public int      AbsentCount      { get; set; }
        public int      OnLeaveCount     { get; set; }
        public double   AttendanceRate   { get; set; }

        public List<AttendanceRowVM> Rows { get; set; } = new();
    }

    public class AttendanceRowVM
    {
        public Guid             EmployeeId     { get; set; }
        public string           EmployeeCode   { get; set; } = "";
        public string           FullNameAr     { get; set; } = "";
        public string           JobTitle       { get; set; } = "";
        public string           ShiftName      { get; set; } = "—";
        public TimeOnly?        CheckIn        { get; set; }
        public TimeOnly?        CheckOut       { get; set; }
        public int              LateMinutes    { get; set; }
        public int              OvertimeMinutes { get; set; }
        public AttendanceStatus Status         { get; set; }
        public bool             HasRecord      { get; set; }
    }
}

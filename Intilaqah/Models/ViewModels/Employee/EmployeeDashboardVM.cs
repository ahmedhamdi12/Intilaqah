using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.Employee
{
    public class EmployeeDashboardVM
    {
        // Identity
        public string  FullNameAr    { get; set; } = "";
        public string  JobTitle      { get; set; } = "";
        public string  EmployeeCode  { get; set; } = "";
        public string  Department    { get; set; } = "";

        // Today's attendance
        public AttendanceStatus? TodayStatus  { get; set; }
        public TimeOnly?         TodayCheckIn { get; set; }
        public TimeOnly?         TodayCheckOut { get; set; }
        public string            ShiftName    { get; set; } = "—";

        // Leave balance
        public int AnnualLeaveDays     { get; set; }
        public int UsedLeaveDays       { get; set; }
        public int RemainingLeaveDays  { get; set; }
        public int PendingLeaveRequests { get; set; }

        // Last payslip
        public string?  LastPayslipMonth   { get; set; }
        public decimal? LastNetSalary      { get; set; }
        public Guid?    LastPayrollRunId   { get; set; }

        // Documents
        public int ExpiringDocumentsCount  { get; set; }
        public List<DocumentAlertItem> ExpiringDocuments { get; set; } = new();

        // Contract
        public DateTime? ContractEndDate   { get; set; }
        public bool      ContractExpiring  { get; set; }
    }

    public class DocumentAlertItem
    {
        public string    DocType      { get; set; } = "";
        public DateTime? ExpiryDate   { get; set; }
        public int       DaysRemaining { get; set; }
    }
}

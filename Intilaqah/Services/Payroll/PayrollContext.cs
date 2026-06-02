using Intilaqah.Models;

namespace Intilaqah.Services.Payroll
{
    public class PayrollContext
    {
        public Employee                Employee    { get; set; } = null!;
        public EmployeeBankAccount?    BankAccount { get; set; }
        public Contract?               Contract    { get; set; }
        public List<AttendanceLog>     Attendance  { get; set; } = new();
        public List<SalaryAdvance>     Advances    { get; set; } = new();
        public List<ViolationRecord>   Violations  { get; set; } = new();
        public int                     WorkingDays { get; set; } = 26;
        public int                     Month       { get; set; }
        public int                     Year        { get; set; }

        // Filled by engine:
        public decimal GrossSalary      { get; set; }
        public decimal TotalDeductions  { get; set; }
        public decimal NetSalary        { get; set; }
        public decimal OvertimeAmount   { get; set; }
        public decimal LateDeduction    { get; set; }
        public decimal AbsenceDeduction { get; set; }
        public decimal ViolationDeduction { get; set; }
        public decimal AdvanceDeduction { get; set; }

        // Helpers
        public decimal BasicSalary => Contract?.BasicSalary ?? Employee.BasicSalary;
        public decimal DailySalary => WorkingDays > 0 ? BasicSalary / WorkingDays : 0;
        public decimal HourlySalary => DailySalary / 8;
        public decimal MinuteSalary => HourlySalary / 60;
    }
}

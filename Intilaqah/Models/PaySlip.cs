using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class PaySlip : BaseEntity
    {
        public Guid    PayrollRunId       { get; set; }
        public Guid    EmployeeId         { get; set; }

        // Employee Snapshot Data
        public string  EmployeeNameAr     { get; set; } = "";
        public string  EmployeeNameEn     { get; set; } = "";
        public string  EmployeeCode       { get; set; } = "";
        public string  NationalId         { get; set; } = "";

        // Earnings
        public decimal BasicSalary        { get; set; }
        public decimal HousingAllowance   { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowances    { get; set; }
        public decimal OvertimeAmount     { get; set; }
        public decimal GrossSalary        { get; set; }

        // Deductions
        public decimal LateDeduction      { get; set; }
        public decimal AbsenceDeduction   { get; set; }
        public decimal ViolationDeduction { get; set; }
        public decimal AdvanceDeduction   { get; set; }
        public decimal TotalDeductions    { get; set; }

        // Result
        public decimal NetSalary          { get; set; }

        // Attendance summary
        public int PresentDays    { get; set; }
        public int AbsentDays     { get; set; }
        public int LateDays       { get; set; }
        public int TotalLateMinutes    { get; set; }
        public int TotalOvertimeMinutes { get; set; }

        // Navigation
        public PayrollRun PayrollRun { get; set; } = null!;
        public Employee   Employee   { get; set; } = null!;
    }
}

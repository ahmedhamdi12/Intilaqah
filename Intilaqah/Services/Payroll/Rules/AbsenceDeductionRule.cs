using Intilaqah.Models;

namespace Intilaqah.Services.Payroll.Rules
{
    public class AbsenceDeductionRule : IPayrollRule
    {
        public string RuleName   => "خصم الغياب";
        public bool   IsAddition => false;

        public Task<decimal> CalculateAsync(PayrollContext context)
        {
            var absentDays = context.Attendance
                .Count(a => a.Status == AttendanceStatus.Absent);

            var deduction = context.DailySalary * absentDays;
            context.AbsenceDeduction = Math.Round(deduction, 2);

            return Task.FromResult(context.AbsenceDeduction);
        }
    }
}

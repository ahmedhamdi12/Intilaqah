using Intilaqah.Models;

namespace Intilaqah.Services.Payroll.Rules
{
    public class LateDeductionRule : IPayrollRule
    {
        public string RuleName   => "خصم التأخر";
        public bool   IsAddition => false;

        public Task<decimal> CalculateAsync(PayrollContext context)
        {
            var totalLateMinutes = context.Attendance
                .Sum(a => a.LateMinutes);

            var deduction = context.MinuteSalary * totalLateMinutes;
            context.LateDeduction = Math.Round(deduction, 2);

            return Task.FromResult(context.LateDeduction);
        }
    }
}

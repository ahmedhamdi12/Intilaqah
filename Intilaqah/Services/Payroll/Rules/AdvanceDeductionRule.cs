using System.Linq;

namespace Intilaqah.Services.Payroll.Rules
{
    public class AdvanceDeductionRule : IPayrollRule
    {
        public string RuleName   => "خصم السلف";
        public bool   IsAddition => false;

        public Task<decimal> CalculateAsync(PayrollContext context)
        {
            var deduction = context.Advances
                .Where(a => a.RemainingAmount > 0)
                .Sum(a => System.Math.Min(a.MonthlyDeduction, a.RemainingAmount));

            context.AdvanceDeduction = System.Math.Round(deduction, 2);

            return Task.FromResult(context.AdvanceDeduction);
        }
    }
}

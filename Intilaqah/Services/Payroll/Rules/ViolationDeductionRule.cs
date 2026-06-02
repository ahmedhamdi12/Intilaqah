namespace Intilaqah.Services.Payroll.Rules
{
    public class ViolationDeductionRule : IPayrollRule
    {
        public string RuleName   => "خصم المخالفات";
        public bool   IsAddition => false;

        public Task<decimal> CalculateAsync(PayrollContext context)
        {
            var deduction = context.Violations
                .Where(v => v.PayrollRunId == null) // Only unprocessed ones
                .Sum(v => v.ViolationRule?.DeductionAmount ?? 0);

            // The deduction in the rule is likely a multiplier of the daily salary or a fixed amount. 
            // In typical Saudi law, the violation deduction amount is a multiplier of the daily wage (e.g. 0.25 = 25% of daily wage).
            var totalDeduction = deduction * context.DailySalary;

            context.ViolationDeduction = Math.Round(totalDeduction, 2);

            return Task.FromResult(context.ViolationDeduction);
        }
    }
}

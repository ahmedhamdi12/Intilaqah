namespace Intilaqah.Services.Payroll
{
    public interface IPayrollEngine
    {
        Task<PayrollContext> RunAsync(PayrollContext context);
    }

    public class PayrollEngine : IPayrollEngine
    {
        private readonly IEnumerable<IPayrollRule> _rules;

        public PayrollEngine(IEnumerable<IPayrollRule> rules)
        {
            _rules = rules;
        }

        public async Task<PayrollContext> RunAsync(PayrollContext context)
        {
            // Set gross salary from contract
            var contract = context.Contract;
            var basic    = contract?.BasicSalary    ?? context.Employee.BasicSalary;
            var housing  = contract?.HousingAllowance   ?? 0;
            var transport = contract?.TransportAllowance ?? 0;
            var other    = contract?.OtherAllowances    ?? 0;

            context.GrossSalary = basic + housing + transport + other;

            // Run each rule
            decimal totalDeductions = 0;
            decimal totalAdditions  = 0;

            foreach (var rule in _rules)
            {
                var amount = await rule.CalculateAsync(context);
                if (rule.IsAddition)
                    totalAdditions  += amount;
                else
                    totalDeductions += amount;
            }

            // Add overtime to gross
            context.GrossSalary  += context.OvertimeAmount;
            context.TotalDeductions = totalDeductions;
            context.NetSalary    = Math.Max(0, context.GrossSalary - totalDeductions);

            return context;
        }
    }
}

namespace Intilaqah.Services.Payroll.Rules
{
    public class OvertimeRule : IPayrollRule
    {
        public string RuleName   => "بدل الإضافي";
        public bool   IsAddition => true;

        public Task<decimal> CalculateAsync(PayrollContext context)
        {
            var totalOvertimeMinutes = context.Attendance
                .Sum(a => a.OvertimeMinutes);

            // Saudi law: overtime = 1.5x hourly rate
            var addition = context.MinuteSalary * 1.5m * totalOvertimeMinutes;
            context.OvertimeAmount = Math.Round(addition, 2);

            return Task.FromResult(context.OvertimeAmount);
        }
    }
}

namespace Intilaqah.Services.Payroll
{
    public interface IPayrollRule
    {
        string  RuleName   { get; }
        bool    IsAddition { get; }  // true = adds to salary, false = deducts
        Task<decimal> CalculateAsync(PayrollContext context);
    }
}

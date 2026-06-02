using Intilaqah.Models;

namespace Intilaqah.Services.Payroll
{
    public interface IPayrollService
    {
        Task<PayrollRun> CreateRunAsync(int month, int year, int workingDays, string createdBy);
        Task<PayrollRun> ApproveRunAsync(Guid payrollRunId, string approvedBy);
    }
}

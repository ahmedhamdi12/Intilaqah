using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IPayrollRepository : IGenericRepository<PayrollRun>
    {
        Task<PayrollRun?> GetWithPaySlipsAsync(Guid id);
        Task<PayrollRun?> GetByMonthYearAsync(int month, int year);
        Task<IEnumerable<PaySlip>> GetPaySlipsByRunAsync(Guid payrollRunId);
        Task<PaySlip?> GetPaySlipByEmployeeRunAsync(Guid employeeId, Guid payrollRunId);
    }
}

using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface ISalaryAdvanceRepository : IGenericRepository<SalaryAdvance>
    {
        Task<IEnumerable<SalaryAdvance>> GetActiveByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<SalaryAdvance>> GetPendingAsync();
    }
}

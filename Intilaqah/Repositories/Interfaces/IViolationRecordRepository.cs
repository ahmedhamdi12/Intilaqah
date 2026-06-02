using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IViolationRecordRepository : IGenericRepository<ViolationRecord>
    {
        Task<IEnumerable<ViolationRecord>> GetByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<ViolationRecord>> GetUnprocessedAsync();
    }
}

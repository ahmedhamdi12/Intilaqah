using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface ISalaryAdvanceTransactionRepository : IGenericRepository<SalaryAdvanceTransaction>
    {
        Task<IEnumerable<SalaryAdvanceTransaction>> GetByAdvanceIdAsync(Guid salaryAdvanceId);
    }
}

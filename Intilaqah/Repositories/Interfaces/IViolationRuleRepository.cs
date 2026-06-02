using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IViolationRuleRepository : IGenericRepository<ViolationRule>
    {
        Task<IEnumerable<ViolationRule>> GetActiveAsync();
    }
}

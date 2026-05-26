using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface ITenantRepository : IGenericRepository<Tenant>
    {
        Task<Tenant?> GetByIdWithPlanAsync(Guid id);
        Task<IEnumerable<Tenant>> GetExpiringContractsAsync(int daysAhead);
        Task<int> CountActiveAsync();
        Task<int> CountFrozenAsync();
        Task<Dictionary<Guid, int>> GetEmployeeCountsPerTenantAsync();
    }
}

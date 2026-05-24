using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IPlanRepository : IGenericRepository<Plan>
    {
        Task<IEnumerable<Plan>> GetActivePlansAsync();
    }
}

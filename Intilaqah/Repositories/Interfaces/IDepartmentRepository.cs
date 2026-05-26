using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<IEnumerable<Department>> GetActiveAsync();
    }
}

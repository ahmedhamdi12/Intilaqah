using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IShiftRepository : IGenericRepository<Shift>
    {
        Task<IEnumerable<Shift>> GetActiveAsync();
    }
}

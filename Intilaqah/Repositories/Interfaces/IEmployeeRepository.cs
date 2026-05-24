using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<int> CountSaudiAsync();
        Task<int> CountNonSaudiAsync();
        Task<decimal> GetSaudizationPercentageAsync();
    }
}

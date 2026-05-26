using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task<Contract?> GetActiveContractAsync(Guid employeeId);
        Task<IEnumerable<Contract>> GetByEmployeeAsync(Guid employeeId);
    }
}

using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IEmployeeBankAccountRepository : IGenericRepository<EmployeeBankAccount>
    {
        Task<EmployeeBankAccount?> GetActiveByEmployeeAsync(Guid employeeId);
    }
}

using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class EmployeeBankAccountRepository : GenericRepository<EmployeeBankAccount>, IEmployeeBankAccountRepository
    {
        public EmployeeBankAccountRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<EmployeeBankAccount?> GetActiveByEmployeeAsync(Guid employeeId)
            => await _dbSet.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.IsActive);
    }
}

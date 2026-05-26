using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class ContractRepository
        : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(
            ApplicationDbContext context,
            ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<Contract?> GetActiveContractAsync(Guid employeeId)
            => await _dbSet
                .Where(c => c.EmployeeId == employeeId && c.IsActive)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<Contract>> GetByEmployeeAsync(Guid employeeId)
            => await _dbSet
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
    }
}

using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class SalaryAdvanceRepository : GenericRepository<SalaryAdvance>, ISalaryAdvanceRepository
    {
        public SalaryAdvanceRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<SalaryAdvance>> GetActiveByEmployeeAsync(Guid employeeId)
            => await _dbSet
                .Where(a => a.EmployeeId == employeeId
                         && a.Status == AdvanceStatus.Approved
                         && a.RemainingAmount > 0)
                .ToListAsync();

        public async Task<IEnumerable<SalaryAdvance>> GetPendingAsync()
            => await _dbSet
                .Where(a => a.Status == AdvanceStatus.Pending)
                .Include(a => a.Employee)
                .OrderByDescending(a => a.RequestDate)
                .ToListAsync();
    }
}

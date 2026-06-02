using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class ViolationRecordRepository : GenericRepository<ViolationRecord>, IViolationRecordRepository
    {
        public ViolationRecordRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<ViolationRecord>> GetByEmployeeAsync(Guid employeeId)
            => await _dbSet
                .Where(v => v.EmployeeId == employeeId)
                .Include(v => v.ViolationRule)
                .OrderByDescending(v => v.ViolationDate)
                .ToListAsync();

        public async Task<IEnumerable<ViolationRecord>> GetUnprocessedAsync()
            => await _dbSet
                .Where(v => v.PayrollRunId == null)
                .Include(v => v.ViolationRule)
                .Include(v => v.Employee)
                .ToListAsync();
    }
}

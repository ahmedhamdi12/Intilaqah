using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
    {
        public TenantRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
         : base(context, tenantResolver) { }

        public async Task<Tenant?> GetByIdWithPlanAsync(Guid id)
            => await _dbSet.Include(t => t.Plan)
                           .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Tenant>> GetExpiringContractsAsync(int daysAhead)
        {
            var cutoff = DateTime.UtcNow.AddDays(daysAhead);
            return await _dbSet
                .Where(t => t.ContractEndDate <= cutoff && t.Status == TenantStatus.Active)
                .OrderBy(t => t.ContractEndDate)
                .ToListAsync();
        }

        public async Task<int> CountActiveAsync()
            => await _dbSet.CountAsync(t => t.Status == TenantStatus.Active);

        public async Task<int> CountFrozenAsync()
            => await _dbSet.CountAsync(t => t.Status == TenantStatus.Frozen);

        public async Task<Dictionary<Guid, int>> GetEmployeeCountsPerTenantAsync()
            => await _context.Tenants
                .Where(t => !t.IsDeleted)
                .Select(t => new {
                    t.Id,
                    Count = t.Employees.Count(e => !e.IsDeleted && e.IsActive)
                })
                .ToDictionaryAsync(x => x.Id, x => x.Count);
    }
}

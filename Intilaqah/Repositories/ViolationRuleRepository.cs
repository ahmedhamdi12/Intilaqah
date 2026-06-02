using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class ViolationRuleRepository : GenericRepository<ViolationRule>, IViolationRuleRepository
    {
        public ViolationRuleRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<ViolationRule>> GetActiveAsync()
            => await _dbSet
                .Where(v => v.IsActive)
                .OrderBy(v => v.RuleNumber)
                .ToListAsync();
    }
}

using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class PlanRepository : GenericRepository<Plan>, IPlanRepository
    {
        public PlanRepository(ApplicationDbContext context, ITenantResolver tenantResolver) : base(context, tenantResolver) { }

        public async Task<IEnumerable<Plan>> GetActivePlansAsync()
        => await _dbSet.Where(p => p.IsActive).ToListAsync();
    }
}

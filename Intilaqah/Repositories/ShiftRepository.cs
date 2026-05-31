using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class ShiftRepository
        : GenericRepository<Shift>, IShiftRepository
    {
        public ShiftRepository(
            ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<Shift>> GetActiveAsync()
            => await _dbSet
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
    }
}

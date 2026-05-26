using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class DepartmentRepository
        : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(
            ApplicationDbContext context,
            ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<Department>> GetActiveAsync()
            => await _dbSet
                .Where(d => d.IsActive && !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToListAsync();
    }
}

using System.Linq.Expressions;
using Intilaqah.Data;
using Intilaqah.Models.Base;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly ITenantResolver _tenantResolver;

        public GenericRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _tenantResolver = tenantResolver;
        }

        // Defense in depth — validate tenant even if EF filter is bypassed
        private void GuardTenant(T entity)
        {
            var tenantId = _tenantResolver.GetTenantId();
            if (tenantId.HasValue && entity.TenantId != Guid.Empty
                && entity.TenantId != tenantId.Value)
            {
                throw new UnauthorizedAccessException(
                    "Cross-tenant access attempt blocked at repository level.");
            }
        }
        public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.Where(e => !e.IsDeleted).ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
        {
            var tenantId = _tenantResolver.GetTenantId();
            if (tenantId.HasValue && entity.TenantId == Guid.Empty)
                entity.TenantId = tenantId.Value;

            GuardTenant(entity);
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            GuardTenant(entity);
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            GuardTenant(entity);
            // Soft delete handled by SaveChangesAsync in DbContext
            _dbSet.Update(entity);
            entity.IsDeleted = true;
        }
    }
}

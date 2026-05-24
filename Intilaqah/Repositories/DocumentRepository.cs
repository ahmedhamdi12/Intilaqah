using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context, ITenantResolver tenantResolver) : base(context, tenantResolver) { }

        public async Task<IEnumerable<Document>> GetExpiringAsync(int daysAhead)
        {
            var cutoff = DateTime.UtcNow.AddDays(daysAhead);
            return await _dbSet
                .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value <= cutoff)
                .OrderBy(d => d.ExpiryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByEntityAsync(Guid entityId, DocumentEntityType type)
            => await _dbSet
                .Where(d => d.EntityId == entityId && d.EntityType == type)
                .ToListAsync();
    }
}

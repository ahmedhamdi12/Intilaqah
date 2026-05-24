using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<IEnumerable<Document>> GetExpiringAsync(int daysAhead);
        Task<IEnumerable<Document>> GetByEntityAsync(Guid entityId, DocumentEntityType type);
    }
}


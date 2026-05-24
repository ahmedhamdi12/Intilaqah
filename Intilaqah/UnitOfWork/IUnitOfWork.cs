using Intilaqah.Repositories.Interfaces;

namespace Intilaqah.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ITenantRepository Tenants { get; }
        IEmployeeRepository Employees { get; }
        IPlanRepository Plans { get; }
        IDocumentRepository Documents { get; }
        IPermissionRepository Permissions { get; }

        Task<int> SaveChangesAsync();
    }
}

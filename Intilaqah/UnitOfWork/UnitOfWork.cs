using Intilaqah.Data;
using Intilaqah.Repositories;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;

namespace Intilaqah.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantResolver _tenantResolver;

        public ITenantRepository Tenants { get; }
        public IEmployeeRepository Employees { get; }
        public IPlanRepository Plans { get; }
        public IDocumentRepository Documents { get; }
        public IPermissionRepository Permissions { get; }

        public UnitOfWork(ApplicationDbContext context, ITenantResolver tenantResolver)
        {
            _context = context;
            _tenantResolver = tenantResolver;
            Tenants = new TenantRepository(context, tenantResolver);
            Employees = new EmployeeRepository(context, tenantResolver);
            Plans = new PlanRepository(context, tenantResolver);
            Documents = new DocumentRepository(context, tenantResolver);
            Permissions = new PermissionRepository(context);
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}

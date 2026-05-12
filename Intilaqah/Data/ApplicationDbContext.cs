using Intilaqah.Models;
using Intilaqah.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantResolver _tenantResolver;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ITenantResolver tenantResolver) : base(options)
        {
            _tenantResolver = tenantResolver;
        }

        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var tenantId = _tenantResolver.GetTenantId();

            // Global Query Filters — this is the magic of multi-tenancy
            if (tenantId.HasValue)
            {
                builder.Entity<Employee>()
                    .HasQueryFilter(e => e.TenantId == tenantId.Value);

                builder.Entity<Document>()
                    .HasQueryFilter(d => d.TenantId == tenantId.Value);
            }
        }
    }
}

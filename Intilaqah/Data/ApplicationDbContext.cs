using Intilaqah.Models;
using Intilaqah.Models.Base;
using Intilaqah.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, AppRole, string>
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
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Composite PK for RolePermission
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Global query filters — tenant isolation + soft delete
            var tenantId = _tenantResolver.GetTenantId();

            if (tenantId.HasValue)
            {
                builder.Entity<Employee>()
                    .HasQueryFilter(e => e.TenantId == tenantId.Value && !e.IsDeleted);

                builder.Entity<Document>()
                    .HasQueryFilter(d => d.TenantId == tenantId.Value && !d.IsDeleted);
            }

            builder.Entity<Tenant>()
                .HasQueryFilter(t => !t.IsDeleted);

            builder.Entity<Plan>()
                .HasQueryFilter(p => !p.IsDeleted);
        }

        // Audit hook — auto-fill CreatedBy / UpdatedBy / DeletedBy
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var currentUser = _tenantResolver.GetCurrentUserId() ?? "system";
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = currentUser;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = currentUser;
                        break;

                    case EntityState.Deleted:
                        // Soft delete — never physically delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = currentUser;
                        break;
                }
            }

            return await base.SaveChangesAsync(ct);
        }
    }
    }

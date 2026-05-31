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

        // EF Core reads this property at query time to parameterize the filter
        private Guid? TenantId => _tenantResolver.GetTenantId();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ITenantResolver tenantResolver) : base(options)
        {
            _tenantResolver = tenantResolver;
        }

        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
        public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Contract>   Contracts   => Set<Contract>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Composite PK for RolePermission
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Global query filters — tenant isolation + soft delete
            // Reference TenantId property (not method call) so EF Core can parameterize it
            builder.Entity<Employee>()
                .HasQueryFilter(e => (TenantId == null || e.TenantId == TenantId) && !e.IsDeleted);

            builder.Entity<Document>()
                .HasQueryFilter(d => (TenantId == null || d.TenantId == TenantId) && !d.IsDeleted);

            builder.Entity<Shift>()
                .HasQueryFilter(s => (TenantId == null || s.TenantId == TenantId) && !s.IsDeleted);

            builder.Entity<ShiftAssignment>()
                .HasQueryFilter(sa => (TenantId == null || sa.TenantId == TenantId) && !sa.IsDeleted);

            builder.Entity<AttendanceLog>()
                .HasQueryFilter(al => (TenantId == null || al.TenantId == TenantId) && !al.IsDeleted);

            builder.Entity<Department>()
                .HasQueryFilter(d => (TenantId == null || d.TenantId == TenantId) && !d.IsDeleted);

            builder.Entity<Contract>()
                .HasQueryFilter(c => (TenantId == null || c.TenantId == TenantId) && !c.IsDeleted);

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

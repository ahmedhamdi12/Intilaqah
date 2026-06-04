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
        private readonly Intilaqah.Infrastructure.Audit.IAuditService? _auditService;

        // EF Core reads this property at query time to parameterize the filter
        private Guid? TenantId => _tenantResolver.GetTenantId();
        private string? CurrentUserId => _tenantResolver.GetCurrentUserId();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ITenantResolver tenantResolver,
            Intilaqah.Infrastructure.Audit.IAuditService? auditService = null) : base(options)
        {
            _tenantResolver = tenantResolver;
            _auditService = auditService;
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

        public DbSet<EmployeeBankAccount> EmployeeBankAccounts => Set<EmployeeBankAccount>();
        public DbSet<ViolationRule>   ViolationRules   => Set<ViolationRule>();
        public DbSet<ViolationRecord> ViolationRecords => Set<ViolationRecord>();
        public DbSet<SalaryAdvance>   SalaryAdvances   => Set<SalaryAdvance>();
        public DbSet<SalaryAdvanceTransaction> SalaryAdvanceTransactions => Set<SalaryAdvanceTransaction>();
        public DbSet<PayrollRun>      PayrollRuns      => Set<PayrollRun>();
        public DbSet<PaySlip>         PaySlips         => Set<PaySlip>();
        public DbSet<AuditLog>        AuditLogs        => Set<AuditLog>();
        public DbSet<Notification>    Notifications    => Set<Notification>();

        public DbSet<Intilaqah.Models.Integration.TenantIntegrationSettings> TenantIntegrationSettings => Set<Intilaqah.Models.Integration.TenantIntegrationSettings>();
        public DbSet<Intilaqah.Models.Integration.IntegrationLog> IntegrationLogs => Set<Intilaqah.Models.Integration.IntegrationLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Composite PK for RolePermission
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<Notification>()
                .HasQueryFilter(n => CurrentUserId == null || n.UserId == CurrentUserId);

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

            builder.Entity<EmployeeBankAccount>()
                .HasQueryFilter(e => (TenantId == null || e.TenantId == TenantId) && !e.IsDeleted);

            builder.Entity<ViolationRule>()
                .HasQueryFilter(v => (TenantId == null || v.TenantId == TenantId || v.TenantId == Guid.Empty) && !v.IsDeleted);

            builder.Entity<ViolationRecord>()
                .HasQueryFilter(vr => (TenantId == null || vr.TenantId == TenantId) && !vr.IsDeleted);

            builder.Entity<SalaryAdvance>()
                .HasQueryFilter(sa => (TenantId == null || sa.TenantId == TenantId) && !sa.IsDeleted);

            builder.Entity<SalaryAdvanceTransaction>()
                .HasQueryFilter(sat => (TenantId == null || sat.TenantId == TenantId) && !sat.IsDeleted);

            builder.Entity<PayrollRun>()
                .HasQueryFilter(pr => (TenantId == null || pr.TenantId == TenantId) && !pr.IsDeleted);

            builder.Entity<PaySlip>()
                .HasQueryFilter(ps => (TenantId == null || ps.TenantId == TenantId) && !ps.IsDeleted);

            builder.Entity<Intilaqah.Models.Integration.TenantIntegrationSettings>()
                .HasQueryFilter(s => (TenantId == null || s.TenantId == TenantId) && !s.IsDeleted);

            builder.Entity<Intilaqah.Models.Integration.TenantIntegrationSettings>()
                .HasIndex(s => new { s.TenantId, s.Provider })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0"); // Soft delete support for unique index
        }

        // Audit hook — auto-fill CreatedBy / UpdatedBy / DeletedBy + Audit Logging
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var currentUser = _tenantResolver.GetCurrentUserId() ?? "system";
            var now = DateTime.UtcNow;

            // Collect audit info ONLY from BaseEntity entries (skip Identity entities)
            var auditEntries = new List<(string Action, string EntityName, string? EntityId, string? OldValues, string? NewValues, Guid? TenantId)>();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State != EntityState.Added && 
                    entry.State != EntityState.Modified && 
                    entry.State != EntityState.Deleted)
                    continue;

                // Determine action
                var action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Deleted => "Delete",
                    EntityState.Modified => "Update",
                    _ => "Unknown"
                };

                // Capture old values BEFORE we modify anything
                string? oldVals = null;
                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var origObj = new Dictionary<string, object?>();
                    foreach (var prop in entry.OriginalValues.Properties)
                        origObj[prop.Name] = entry.OriginalValues[prop];
                    oldVals = System.Text.Json.JsonSerializer.Serialize(origObj);
                }

                // Apply BaseEntity timestamps
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
                        action = "Delete";
                        break;
                }

                // Check if this is a soft-delete via IsDeleted flag (from GenericRepository)
                if (entry.State == EntityState.Modified && entry.Entity.IsDeleted && action == "Update")
                {
                    action = "Delete";
                }

                // Capture new values AFTER applying timestamps
                string? newVals = null;
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var currObj = new Dictionary<string, object?>();
                    foreach (var prop in entry.CurrentValues.Properties)
                        currObj[prop.Name] = entry.CurrentValues[prop];
                    newVals = System.Text.Json.JsonSerializer.Serialize(currObj);
                }

                var entityName = entry.Entity.GetType().Name;
                var entityId = entry.Entity.Id.ToString();
                Guid? tenantId = entry.Entity.TenantId;

                auditEntries.Add((action, entityName, entityId, oldVals, newVals, tenantId));
            }

            // Save all changes to the database
            var result = await base.SaveChangesAsync(ct);

            // Log audit entries via AuditService (raw ADO.NET — completely separate)
            if (_auditService != null && auditEntries.Count > 0)
            {
                Console.WriteLine($"[DbContext] Saving {auditEntries.Count} audit entries...");
                foreach (var log in auditEntries)
                {
                    try
                    {
                        await _auditService.LogAsync(
                            log.Action, log.EntityName, log.EntityId,
                            log.OldValues, log.NewValues, log.TenantId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DbContext AuditLog ERROR] {ex.Message}");
                    }
                }
            }

            return result;
        }
    }
}

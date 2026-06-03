using Intilaqah.Data;
using Intilaqah.Repositories;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Intilaqah.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Intilaqah.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantResolver _tenantResolver;
        private readonly IAuditService _auditService;

        public ITenantRepository Tenants { get; }
        public IEmployeeRepository Employees { get; }
        public IPlanRepository Plans { get; }
        public IDocumentRepository Documents { get; }
        public IPermissionRepository Permissions { get; }
        public IDepartmentRepository Departments { get; }
        public IContractRepository   Contracts   { get; }
        public IShiftRepository Shifts { get; }
        public IShiftAssignmentRepository ShiftAssignments { get; }
        public IAttendanceRepository Attendance { get; }
        public IEmployeeBankAccountRepository EmployeeBankAccounts { get; }
        public IViolationRuleRepository   ViolationRules   { get; }
        public IViolationRecordRepository ViolationRecords { get; }
        public ISalaryAdvanceRepository   SalaryAdvances   { get; }
        public ISalaryAdvanceTransactionRepository SalaryAdvanceTransactions { get; }
        public IPayrollRepository         Payroll          { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            ITenantResolver tenantResolver,
            IAuditService auditService)
        {
            _context = context;
            _tenantResolver = tenantResolver;
            _auditService = auditService;
            Tenants = new TenantRepository(context, tenantResolver);
            Employees = new EmployeeRepository(context, tenantResolver);
            Plans = new PlanRepository(context, tenantResolver);
            Documents = new DocumentRepository(context, tenantResolver);
            Permissions = new PermissionRepository(context);
            Departments = new DepartmentRepository(context, tenantResolver);
            Contracts   = new ContractRepository(context, tenantResolver);
            Shifts = new ShiftRepository(context, tenantResolver);
            ShiftAssignments = new ShiftAssignmentRepository(context, tenantResolver);
            Attendance = new AttendanceRepository(context, tenantResolver);
            EmployeeBankAccounts = new EmployeeBankAccountRepository(context, tenantResolver);
            ViolationRules   = new ViolationRuleRepository(context, tenantResolver);
            ViolationRecords = new ViolationRecordRepository(context, tenantResolver);
            SalaryAdvances   = new SalaryAdvanceRepository(context, tenantResolver);
            SalaryAdvanceTransactions = new SalaryAdvanceTransactionRepository(context, tenantResolver);
            Payroll          = new PayrollRepository(context, tenantResolver);
        }

        public async Task<int> SaveChangesAsync()
        {
            var entriesToAudit = _context.ChangeTracker.Entries()
                .Where(e => e.Entity is not Intilaqah.Models.AuditLog && 
                            e.Entity is not Intilaqah.Models.Notification &&
                            (e.State == EntityState.Added || 
                             e.State == EntityState.Modified || 
                             e.State == EntityState.Deleted))
                .ToList();

            var auditLogsInfo = new List<(string Action, string EntityName, string? EntityId, string? OldValues, string? NewValues, Guid? TenantId)>();

            foreach (var entry in entriesToAudit)
            {
                var action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Deleted => "Delete",
                    EntityState.Modified => "Update",
                    _ => "Unknown"
                };

                if (entry.State == EntityState.Modified && entry.Entity is Intilaqah.Models.Base.BaseEntity be && be.IsDeleted)
                {
                    action = "Delete";
                }

                var entityName = entry.Entity.GetType().Name;
                
                var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                var entityId = idProperty?.CurrentValue?.ToString();

                var tenantIdProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
                Guid? tenantId = tenantIdProp?.CurrentValue as Guid?;

                string? oldValues = null;
                string? newValues = null;

                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var originalObj = new Dictionary<string, object?>();
                    foreach (var prop in entry.OriginalValues.Properties)
                    {
                        originalObj[prop.Name] = entry.OriginalValues[prop];
                    }
                    oldValues = JsonSerializer.Serialize(originalObj);
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var currentObj = new Dictionary<string, object?>();
                    foreach (var prop in entry.CurrentValues.Properties)
                    {
                        currentObj[prop.Name] = entry.CurrentValues[prop];
                    }
                    newValues = JsonSerializer.Serialize(currentObj);
                }

                auditLogsInfo.Add((action, entityName, entityId, oldValues, newValues, tenantId));
            }

            var result = await _context.SaveChangesAsync();

            // Run audit logging after successful save, using fire-and-forget or awaited tasks
            foreach (var log in auditLogsInfo)
            {
                // We await to ensure they complete, but AuditService has try/catch internally
                await _auditService.LogAsync(log.Action, log.EntityName, log.EntityId, log.OldValues, log.NewValues, log.TenantId);
            }

            return result;
        }

        public void Dispose()
            => _context.Dispose();
    }
}

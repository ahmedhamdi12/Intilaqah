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
            ITenantResolver tenantResolver)
        {
            _context = context;
            _tenantResolver = tenantResolver;
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

        // All audit logging is now handled inside ApplicationDbContext.SaveChangesAsync
        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}

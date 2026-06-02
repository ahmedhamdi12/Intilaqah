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
        IDepartmentRepository Departments { get; }
        IContractRepository   Contracts   { get; }
        IShiftRepository Shifts { get; }
        IShiftAssignmentRepository ShiftAssignments { get; }
        IAttendanceRepository Attendance { get; }
        IEmployeeBankAccountRepository EmployeeBankAccounts { get; }
        IViolationRuleRepository   ViolationRules   { get; }
        IViolationRecordRepository ViolationRecords { get; }
        ISalaryAdvanceRepository   SalaryAdvances   { get; }
        ISalaryAdvanceTransactionRepository SalaryAdvanceTransactions { get; }
        IPayrollRepository         Payroll          { get; }

        Task<int> SaveChangesAsync();
    }
}

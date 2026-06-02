using Intilaqah.Models;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;

namespace Intilaqah.Services.Payroll
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork     _uow;
        private readonly IPayrollEngine  _engine;
        private readonly ITenantResolver _tenantResolver;

        public PayrollService(IUnitOfWork uow, IPayrollEngine engine, ITenantResolver tenantResolver)
        {
            _uow            = uow;
            _engine          = engine;
            _tenantResolver  = tenantResolver;
        }

        public async Task<PayrollRun> CreateRunAsync(int month, int year, int workingDays, string createdBy)
        {
            // Prevent duplicate run for same month/year
            var existing = await _uow.Payroll.GetByMonthYearAsync(month, year);
            if (existing != null)
                throw new InvalidOperationException($"يوجد مسير رواتب لشهر {month}/{year} مسبقاً");

            // Bulk load all required data
            var employees = (await _uow.Employees.FindAsync(e => e.IsActive)).ToList();
            var employeeIds = employees.Select(e => e.Id).ToList();

            var from = new DateTime(year, month, 1);
            var to   = from.AddMonths(1).AddDays(-1);

            var contracts = (await _uow.Contracts.FindAsync(c => c.IsActive && employeeIds.Contains(c.EmployeeId)))
                .ToDictionary(c => c.EmployeeId);

            var bankAccounts = (await _uow.EmployeeBankAccounts.FindAsync(b => b.IsActive && employeeIds.Contains(b.EmployeeId)))
                .ToDictionary(b => b.EmployeeId);

            var attendance = (await _uow.Attendance.FindAsync(a => a.Date >= from && a.Date <= to && employeeIds.Contains(a.EmployeeId)))
                .GroupBy(a => a.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var advances = (await _uow.SalaryAdvances.FindAsync(a => a.Status == AdvanceStatus.Approved && a.RemainingAmount > 0 && employeeIds.Contains(a.EmployeeId)))
                .GroupBy(a => a.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Unprocessed violations up to the end of the month
            var violationsList = await _uow.ViolationRecords.GetUnprocessedAsync();
            var violations = violationsList
                .Where(v => v.ViolationDate <= to && employeeIds.Contains(v.EmployeeId))
                .GroupBy(v => v.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var tenantId = _tenantResolver.GetTenantId() ?? Guid.Empty;
            var paySlips = new List<PaySlip>();

            foreach (var emp in employees)
            {
                contracts.TryGetValue(emp.Id, out var contract);
                bankAccounts.TryGetValue(emp.Id, out var bankAccount);
                attendance.TryGetValue(emp.Id, out var empAttendance);
                advances.TryGetValue(emp.Id, out var empAdvances);
                violations.TryGetValue(emp.Id, out var empViolations);

                var ctx = new PayrollContext
                {
                    Employee    = emp,
                    BankAccount = bankAccount,
                    Contract    = contract,
                    Attendance  = empAttendance ?? new List<AttendanceLog>(),
                    Advances    = empAdvances ?? new List<SalaryAdvance>(),
                    Violations  = empViolations ?? new List<ViolationRecord>(),
                    WorkingDays = workingDays,
                    Month       = month,
                    Year        = year,
                };

                await _engine.RunAsync(ctx);

                paySlips.Add(new PaySlip
                {
                    TenantId           = tenantId,
                    EmployeeId         = emp.Id,
                    EmployeeNameAr     = emp.FullNameAr,
                    EmployeeNameEn     = emp.FullNameEn,
                    EmployeeCode       = emp.EmployeeCode,
                    NationalId         = emp.NationalId,
                    BasicSalary        = ctx.BasicSalary,
                    HousingAllowance   = contract?.HousingAllowance   ?? 0,
                    TransportAllowance = contract?.TransportAllowance ?? 0,
                    OtherAllowances    = contract?.OtherAllowances    ?? 0,
                    OvertimeAmount     = ctx.OvertimeAmount,
                    GrossSalary        = ctx.GrossSalary,
                    LateDeduction      = ctx.LateDeduction,
                    AbsenceDeduction   = ctx.AbsenceDeduction,
                    ViolationDeduction = ctx.ViolationDeduction,
                    AdvanceDeduction   = ctx.AdvanceDeduction,
                    TotalDeductions    = ctx.TotalDeductions,
                    NetSalary          = ctx.NetSalary,
                    PresentDays        = ctx.Attendance.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentDays         = ctx.Attendance.Count(a => a.Status == AttendanceStatus.Absent),
                    LateDays           = ctx.Attendance.Count(a => a.Status == AttendanceStatus.Late),
                    TotalLateMinutes    = ctx.Attendance.Sum(a => a.LateMinutes),
                    TotalOvertimeMinutes = ctx.Attendance.Sum(a => a.OvertimeMinutes),
                    CreatedBy          = createdBy,
                });
            }

            var run = new PayrollRun
            {
                TenantId     = tenantId,
                Month        = month,
                Year         = year,
                WorkingDays  = workingDays,
                Status       = PayrollStatus.Draft,
                TotalGross   = paySlips.Sum(p => p.GrossSalary),
                TotalDeductions = paySlips.Sum(p => p.TotalDeductions),
                TotalNet     = paySlips.Sum(p => p.NetSalary),
                PaySlips     = paySlips,
                CreatedBy    = createdBy,
            };

            // BaseEntity.Id is auto-generated via Guid.NewGuid() in the constructor,
            // so run.Id is already set before SaveChanges.
            await _uow.Payroll.AddAsync(run);
            await _uow.SaveChangesAsync();

            // Link violations and create advance transactions
            foreach (var emp in employees)
            {
                if (violations.TryGetValue(emp.Id, out var empViolations2))
                {
                    foreach (var v in empViolations2)
                    {
                        v.PayrollRunId = run.Id;
                        _uow.ViolationRecords.Update(v);
                    }
                }

                if (advances.TryGetValue(emp.Id, out var empAdvances2))
                {
                    foreach (var adv in empAdvances2)
                    {
                        var deductedAmount = Math.Min(adv.MonthlyDeduction, adv.RemainingAmount);
                        if (deductedAmount > 0)
                        {
                            var tx = new SalaryAdvanceTransaction
                            {
                                TenantId        = tenantId,
                                SalaryAdvanceId = adv.Id,
                                PayrollRunId    = run.Id,
                                Amount          = deductedAmount,
                                TransactionDate = DateTime.UtcNow,
                                CreatedBy       = createdBy
                            };
                            await _uow.SalaryAdvanceTransactions.AddAsync(tx);
                        }
                    }
                }
            }

            await _uow.SaveChangesAsync();

            return run;
        }

        public async Task<PayrollRun> ApproveRunAsync(Guid payrollRunId, string approvedBy)
        {
            var run = await _uow.Payroll.GetWithPaySlipsAsync(payrollRunId)
                ?? throw new KeyNotFoundException("مسير الرواتب غير موجود");

            if (run.Status == PayrollStatus.Approved || run.Status == PayrollStatus.Exported)
                throw new InvalidOperationException("المسير معتمد مسبقاً");

            run.Status     = PayrollStatus.Approved;
            run.ApprovedAt = DateTime.UtcNow;
            run.ApprovedBy = approvedBy;
            run.UpdatedBy  = approvedBy;
            run.UpdatedAt  = DateTime.UtcNow;

            // Apply advance deductions to the RemainingAmount
            // Find all transactions for this run
            var txs = await _uow.SalaryAdvanceTransactions.FindAsync(t => t.PayrollRunId == payrollRunId);
            foreach (var tx in txs)
            {
                var adv = await _uow.SalaryAdvances.GetByIdAsync(tx.SalaryAdvanceId);
                if (adv != null)
                {
                    adv.RemainingAmount -= tx.Amount;
                    if (adv.RemainingAmount <= 0)
                    {
                        adv.RemainingAmount = 0;
                        adv.Status = AdvanceStatus.FullyDeducted;
                    }
                    adv.UpdatedBy = approvedBy;
                    adv.UpdatedAt = DateTime.UtcNow;
                    _uow.SalaryAdvances.Update(adv);
                }
            }

            _uow.Payroll.Update(run);
            await _uow.SaveChangesAsync();

            return run;
        }
    }
}

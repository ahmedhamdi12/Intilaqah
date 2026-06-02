using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class PayrollRepository : GenericRepository<PayrollRun>, IPayrollRepository
    {
        public PayrollRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<PayrollRun?> GetWithPaySlipsAsync(Guid id)
            => await _dbSet
                .Include(p => p.PaySlips)
                    .ThenInclude(ps => ps.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<PayrollRun?> GetByMonthYearAsync(int month, int year)
            => await _dbSet
                .FirstOrDefaultAsync(p => p.Month == month && p.Year == year);

        public async Task<IEnumerable<PaySlip>> GetPaySlipsByRunAsync(Guid payrollRunId)
            => await _context.Set<PaySlip>()
                .Where(ps => ps.PayrollRunId == payrollRunId && !ps.IsDeleted)
                .Include(ps => ps.Employee)
                .OrderBy(ps => ps.Employee.FullNameAr)
                .ToListAsync();

        public async Task<PaySlip?> GetPaySlipByEmployeeRunAsync(Guid employeeId, Guid payrollRunId)
            => await _context.Set<PaySlip>()
                .FirstOrDefaultAsync(ps =>
                    ps.EmployeeId == employeeId
                    && ps.PayrollRunId == payrollRunId
                    && !ps.IsDeleted);
    }
}

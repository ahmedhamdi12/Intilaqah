using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class AttendanceRepository
        : GenericRepository<AttendanceLog>, IAttendanceRepository
    {
        public AttendanceRepository(
            ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<AttendanceLog>> GetByDateAsync(DateTime date)
            => await _dbSet
                .Where(a => a.Date.Date == date.Date && !a.IsDeleted)
                .Include(a => a.Employee)
                .ToListAsync();

        public async Task<IEnumerable<AttendanceLog>> GetByEmployeeAsync(
            Guid employeeId, DateTime from, DateTime to)
            => await _dbSet
                .Where(a => a.EmployeeId == employeeId
                         && a.Date.Date >= from.Date
                         && a.Date.Date <= to.Date
                         && !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

        public async Task<IEnumerable<AttendanceLog>> GetByDateRangeAsync(
            DateTime from, DateTime to)
            => await _dbSet
                .Where(a => a.Date.Date >= from.Date && a.Date.Date <= to.Date && !a.IsDeleted)
                .Include(a => a.Employee)
                .ToListAsync();

        public async Task<AttendanceLog?> GetByEmployeeDateAsync(
            Guid employeeId, DateTime date)
            => await _dbSet
                .FirstOrDefaultAsync(a =>
                    a.EmployeeId == employeeId && a.Date.Date == date.Date && !a.IsDeleted);
    }
}

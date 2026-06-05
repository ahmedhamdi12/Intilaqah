using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(
            ApplicationDbContext context,
            ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId)
            => await _dbSet
                .Where(l => l.EmployeeId == employeeId)
                .Include(l => l.Employee)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
            => await _dbSet
                .Where(l => l.Status == LeaveRequestStatus.Pending)
                .Include(l => l.Employee)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveRequestStatus status)
            => await _dbSet
                .Where(l => l.Status == status)
                .Include(l => l.Employee)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

        public async Task<bool> HasOverlapAsync(
            Guid employeeId, DateTime start, DateTime end, Guid? excludeId = null)
            => await _dbSet
                .AnyAsync(l =>
                    l.EmployeeId == employeeId
                    && l.Id != excludeId
                    && l.Status != LeaveRequestStatus.Rejected
                    && l.Status != LeaveRequestStatus.Cancelled
                    && l.StartDate <= end
                    && l.EndDate >= start);
    }
}

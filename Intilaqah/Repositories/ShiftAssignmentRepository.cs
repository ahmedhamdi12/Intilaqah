using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class ShiftAssignmentRepository
        : GenericRepository<ShiftAssignment>, IShiftAssignmentRepository
    {
        public ShiftAssignmentRepository(
            ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<ShiftAssignment?> GetActiveByEmployeeAsync(Guid employeeId)
            => await _dbSet
                .Where(sa => sa.EmployeeId == employeeId && sa.IsActive && !sa.IsDeleted)
                .Include(sa => sa.Shift)
                .FirstOrDefaultAsync();
    }
}

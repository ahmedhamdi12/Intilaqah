using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IShiftAssignmentRepository : IGenericRepository<ShiftAssignment>
    {
        Task<ShiftAssignment?> GetActiveByEmployeeAsync(Guid employeeId);
    }
}

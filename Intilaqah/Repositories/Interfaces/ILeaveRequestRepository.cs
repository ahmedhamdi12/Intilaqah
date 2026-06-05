using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
    {
        Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<LeaveRequest>> GetPendingAsync();
        Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveRequestStatus status);
        Task<bool> HasOverlapAsync(Guid employeeId, DateTime start, DateTime end, Guid? excludeId = null);
    }
}

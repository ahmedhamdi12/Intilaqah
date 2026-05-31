using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IAttendanceRepository : IGenericRepository<AttendanceLog>
    {
        Task<IEnumerable<AttendanceLog>> GetByDateAsync(DateTime date);
        Task<IEnumerable<AttendanceLog>> GetByEmployeeAsync(
            Guid employeeId, DateTime from, DateTime to);
        Task<IEnumerable<AttendanceLog>> GetByDateRangeAsync(
            DateTime from, DateTime to);
        Task<AttendanceLog?> GetByEmployeeDateAsync(
            Guid employeeId, DateTime date);
    }
}

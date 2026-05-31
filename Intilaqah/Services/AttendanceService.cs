using Intilaqah.Models;
using Intilaqah.UnitOfWork;

namespace Intilaqah.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceLog> CalculateAsync(
            Guid employeeId, DateTime date,
            TimeOnly? checkIn, TimeOnly? checkOut);
        Task<List<AttendanceLog>> ImportFromExcelAsync(
            Stream excelStream, string importedBy);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _uow;

        public AttendanceService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AttendanceLog> CalculateAsync(
            Guid employeeId, DateTime date,
            TimeOnly? checkIn, TimeOnly? checkOut)
        {
            var assignment = await _uow.ShiftAssignments
                .GetActiveByEmployeeAsync(employeeId);
            var shift = assignment?.Shift;

            var log = new AttendanceLog
            {
                EmployeeId = employeeId,
                Date       = date.Date,
                CheckIn    = checkIn,
                CheckOut   = checkOut,
            };

            if (checkIn == null)
            {
                log.Status      = AttendanceStatus.Absent;
                log.LateMinutes = 0;
                return log;
            }

            if (shift != null)
            {
                // Late calculation
                var gracedStart = shift.StartTime.AddMinutes(shift.GraceMinutes);
                if (checkIn.Value > gracedStart)
                {
                    log.LateMinutes = (int)(checkIn.Value - shift.StartTime)
                        .TotalMinutes;
                    log.Status = AttendanceStatus.Late;
                }
                else
                {
                    log.Status = AttendanceStatus.Present;
                }

                // Overtime calculation (minimum 30 minutes after shift end)
                if (checkOut.HasValue
                    && checkOut.Value > shift.EndTime.AddMinutes(30))
                {
                    log.OvertimeMinutes = (int)(checkOut.Value - shift.EndTime)
                        .TotalMinutes;
                }
            }
            else
            {
                // No shift assigned — just mark present
                log.Status = AttendanceStatus.Present;
            }

            return log;
        }

        public async Task<List<AttendanceLog>> ImportFromExcelAsync(
            Stream excelStream, string importedBy)
        {
            var results  = new List<AttendanceLog>();
            var employees = (await _uow.Employees.GetAllAsync())
                .ToDictionary(e => e.EmployeeCode, e => e.Id);

            using var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);
            var ws = workbook.Worksheet(1);
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                var empCode  = ws.Cell(row, 1).GetString().Trim();
                var dateStr  = ws.Cell(row, 2).GetString().Trim();
                var inStr    = ws.Cell(row, 3).GetString().Trim();
                var outStr   = ws.Cell(row, 4).GetString().Trim();

                if (string.IsNullOrEmpty(empCode) || string.IsNullOrEmpty(dateStr))
                    continue;

                if (!employees.TryGetValue(empCode, out var employeeId))
                    continue; // skip unknown employee codes

                if (!DateTime.TryParse(dateStr, out var date))
                    continue;

                TimeOnly? checkIn  = TimeOnly.TryParse(inStr,  out var ci) ? ci : null;
                TimeOnly? checkOut = TimeOnly.TryParse(outStr, out var co) ? co : null;

                var log = await CalculateAsync(employeeId, date, checkIn, checkOut);
                
                var existing = await _uow.Attendance
                    .GetByEmployeeDateAsync(employeeId, date);
                
                if (existing != null)
                {
                    existing.CheckIn         = log.CheckIn;
                    existing.CheckOut        = log.CheckOut;
                    existing.LateMinutes     = log.LateMinutes;
                    existing.OvertimeMinutes = log.OvertimeMinutes;
                    existing.Status          = log.Status;
                    existing.ImportSource    = "Excel Import (Updated)";
                    existing.UpdatedBy       = importedBy;
                    existing.UpdatedAt       = DateTime.UtcNow;
                    
                    _uow.Attendance.Update(existing);
                    results.Add(existing);
                }
                else
                {
                    log.ImportSource = "Excel Import";
                    log.CreatedBy    = importedBy;

                    results.Add(log);
                    await _uow.Attendance.AddAsync(log);
                }
            }

            await _uow.SaveChangesAsync();
            return results;
        }

        private static bool IsWeekend(DateTime date)
            => date.DayOfWeek == DayOfWeek.Friday
            || date.DayOfWeek == DayOfWeek.Saturday;
    }
}

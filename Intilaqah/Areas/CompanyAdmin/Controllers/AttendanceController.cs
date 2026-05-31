using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class AttendanceController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(
            IUnitOfWork uow, IAttendanceService attendanceService)
        {
            _uow               = uow;
            _attendanceService = attendanceService;
        }

        // GET /CompanyAdmin/Attendance
        // Daily attendance dashboard
        public async Task<IActionResult> Index(DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.Today;
            var employees  = (await _uow.Employees
                .FindAsync(e => e.IsActive)).ToList();
            var logs       = (await _uow.Attendance
                .GetByDateAsync(targetDate)).ToList();
            var assignments = new Dictionary<Guid, string>();

            foreach (var emp in employees)
            {
                var sa = await _uow.ShiftAssignments
                    .GetActiveByEmployeeAsync(emp.Id);
                assignments[emp.Id] = sa?.Shift?.Name ?? "—";
            }

            var rows = employees.Select(emp => {
                var log = logs.FirstOrDefault(l => l.EmployeeId == emp.Id);
                return new AttendanceRowVM
                {
                    EmployeeId      = emp.Id,
                    EmployeeCode    = emp.EmployeeCode,
                    FullNameAr      = emp.FullNameAr,
                    JobTitle        = emp.JobTitle ?? "",
                    ShiftName       = assignments.GetValueOrDefault(emp.Id, "—"),
                    CheckIn         = log?.CheckIn,
                    CheckOut        = log?.CheckOut,
                    LateMinutes     = log?.LateMinutes     ?? 0,
                    OvertimeMinutes = log?.OvertimeMinutes ?? 0,
                    Status          = log?.Status ?? AttendanceStatus.Absent,
                    HasRecord       = log != null,
                };
            }).ToList();

            var vm = new AttendanceDashboardVM
            {
                Date           = targetDate,
                TotalEmployees = employees.Count,
                PresentCount   = rows.Count(r =>
                    r.Status == AttendanceStatus.Present),
                LateCount      = rows.Count(r =>
                    r.Status == AttendanceStatus.Late),
                AbsentCount    = rows.Count(r =>
                    r.Status == AttendanceStatus.Absent),
                OnLeaveCount   = rows.Count(r =>
                    r.Status == AttendanceStatus.OnLeave),
                AttendanceRate = employees.Count == 0 ? 0 :
                    Math.Round((double)rows.Count(r =>
                        r.Status != AttendanceStatus.Absent)
                        / employees.Count * 100, 1),
                Rows = rows.OrderBy(r => r.Status).ToList(),
            };

            return View(vm);
        }

        // GET /CompanyAdmin/Attendance/Import
        public IActionResult Import() => View();

        // POST /CompanyAdmin/Attendance/Import
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "يرجى اختيار ملف Excel";
                return View();
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["Error"] = "يجب أن يكون الملف بصيغة Excel (.xlsx)";
                return View();
            }

            try
            {
                var importedBy = User.FindFirst("FullName")?.Value ?? "system";
                await using var stream = file.OpenReadStream();
                var results = await _attendanceService
                    .ImportFromExcelAsync(stream, importedBy);

                TempData["Success"] =
                    $"تم استيراد {results.Count} سجل بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"خطأ أثناء الاستيراد: {ex.Message}";
                return View();
            }
        }

        // GET /CompanyAdmin/Attendance/Employee/id?from=&to=
        public async Task<IActionResult> Employee(
            Guid id, DateTime? from, DateTime? to)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null) return NotFound();

            var fromDate = from?.Date ?? DateTime.Today.AddDays(-30);
            var toDate   = to?.Date   ?? DateTime.Today;

            var logs = (await _uow.Attendance
                .GetByEmployeeAsync(id, fromDate, toDate)).ToList();

            ViewBag.Employee = employee;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate   = toDate;
            ViewBag.TotalLate     = logs.Sum(l => l.LateMinutes);
            ViewBag.TotalOvertime = logs.Sum(l => l.OvertimeMinutes);
            ViewBag.AbsentDays    = logs.Count(
                l => l.Status == AttendanceStatus.Absent);

            return View(logs);
        }

        // POST /CompanyAdmin/Attendance/AssignShift
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShift(
            Guid employeeId, Guid shiftId)
        {
            var currentUser = User.FindFirst("FullName")?.Value ?? "system";

            // Deactivate existing assignments
            var existing = (await _uow.ShiftAssignments
                .FindAsync(sa => sa.EmployeeId == employeeId
                              && sa.IsActive)).ToList();
            foreach (var old in existing)
            {
                old.IsActive  = false;
                old.UpdatedBy = currentUser;
                old.UpdatedAt = DateTime.UtcNow;
                _uow.ShiftAssignments.Update(old);
            }

            // Create new assignment
            var assignment = new ShiftAssignment
            {
                EmployeeId    = employeeId,
                ShiftId       = shiftId,
                EffectiveFrom = DateTime.Today,
                IsActive      = true,
                CreatedBy     = currentUser,
            };

            await _uow.ShiftAssignments.AddAsync(assignment);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تعيين الوردية بنجاح";
            return RedirectToAction("Details", "Employees",
                new { id = employeeId });
        }
    }
}

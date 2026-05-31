using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class ShiftsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ShiftsController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index()
        {
            var shifts = (await _uow.Shifts.GetAllAsync()).ToList();
            return View(shifts);
        }

        public IActionResult Create() => View(new ShiftCreateVM());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShiftCreateVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var shift = new Shift
            {
                Name         = model.Name,
                ShiftType    = model.ShiftType,
                StartTime    = model.StartTime,
                EndTime      = model.EndTime,
                GraceMinutes = model.GraceMinutes,
                IsActive     = model.IsActive,
                CreatedBy    = User.FindFirst("FullName")?.Value ?? "system",
            };

            await _uow.Shifts.AddAsync(shift);
            await _uow.SaveChangesAsync();

            TempData["Success"] = $"تم إنشاء وردية '{model.Name}' بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var shift = await _uow.Shifts.GetByIdAsync(id);
            if (shift == null) return NotFound();

            var vm = new ShiftCreateVM
            {
                Name         = shift.Name,
                ShiftType    = shift.ShiftType,
                StartTime    = shift.StartTime,
                EndTime      = shift.EndTime,
                GraceMinutes = shift.GraceMinutes,
                IsActive     = shift.IsActive,
            };
            ViewBag.ShiftId = id;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ShiftCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShiftId = id;
                return View(model);
            }

            var shift = await _uow.Shifts.GetByIdAsync(id);
            if (shift == null) return NotFound();

            shift.Name         = model.Name;
            shift.ShiftType    = model.ShiftType;
            shift.StartTime    = model.StartTime;
            shift.EndTime      = model.EndTime;
            shift.GraceMinutes = model.GraceMinutes;
            shift.IsActive     = model.IsActive;
            shift.UpdatedBy    = User.FindFirst("FullName")?.Value ?? "system";
            shift.UpdatedAt    = DateTime.UtcNow;

            _uow.Shifts.Update(shift);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الوردية";
            return RedirectToAction(nameof(Index));
        }
    }
}

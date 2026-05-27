using Intilaqah.Models;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class DepartmentsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DepartmentsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /CompanyAdmin/Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _uow.Departments.GetAllAsync();
            return View(departments);
        }

        // POST: /CompanyAdmin/Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "اسم القسم مطلوب";
                return RedirectToAction(nameof(Index));
            }

            var dept = new Department
            {
                Name      = name.Trim(),
                IsActive  = true,
                CreatedBy = User.FindFirst("FullName")?.Value ?? "system"
            };
            await _uow.Departments.AddAsync(dept);
            await _uow.SaveChangesAsync();

            TempData["Success"] = $"تم إضافة قسم «{dept.Name}» بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // POST: /CompanyAdmin/Departments/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string name)
        {
            var dept = await _uow.Departments.GetByIdAsync(id);
            if (dept == null) return NotFound();

            dept.Name      = name.Trim();
            dept.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            dept.UpdatedAt = DateTime.UtcNow;

            _uow.Departments.Update(dept);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث القسم";
            return RedirectToAction(nameof(Index));
        }

        // POST: /CompanyAdmin/Departments/ToggleActive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var dept = await _uow.Departments.GetByIdAsync(id);
            if (dept == null) return NotFound();

            dept.IsActive  = !dept.IsActive;
            dept.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            dept.UpdatedAt = DateTime.UtcNow;

            _uow.Departments.Update(dept);
            await _uow.SaveChangesAsync();

            TempData["Success"] = dept.IsActive
                ? $"تم تفعيل قسم «{dept.Name}»"
                : $"تم تعطيل قسم «{dept.Name}»";

            return RedirectToAction(nameof(Index));
        }
    }
}

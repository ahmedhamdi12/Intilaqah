using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class DepartmentsController : Controller
    {
        private readonly IUnitOfWork                  _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentsController(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager)
        {
            _uow         = uow;
            _userManager = userManager;
        }

        // GET /CompanyAdmin/Departments
        public async Task<IActionResult> Index()
        {
            var departments = (await _uow.Departments.GetAllAsync()).ToList();
            var employees   = (await _uow.Employees.GetAllAsync()).ToList();

            // Build manager name lookup
            var managerIds = departments
                .Where(d => d.ManagerId != null)
                .Select(d => d.ManagerId!)
                .Distinct()
                .ToList();

            var managerNames = new Dictionary<string, string>();
            foreach (var mId in managerIds)
            {
                var user = await _userManager.FindByIdAsync(mId);
                if (user != null)
                    managerNames[mId] = user.FullName ?? user.Email ?? mId;
            }

            var vm = departments.Select(d => new DepartmentListItemVM
            {
                Id            = d.Id,
                Name          = d.Name,
                ManagerName   = d.ManagerId != null
                    ? managerNames.GetValueOrDefault(d.ManagerId, "—")
                    : "—",
                EmployeeCount = employees.Count(e => e.DepartmentId == d.Id),
                IsActive      = d.IsActive,
                CreatedAt     = d.CreatedAt,
            }).OrderBy(d => d.Name).ToList();

            ViewBag.TotalCount  = vm.Count;
            ViewBag.ActiveCount = vm.Count(d => d.IsActive);

            return View(vm);
        }

        // GET /CompanyAdmin/Departments/Create
        public IActionResult Create()
        {
            return View(BuildCreateVM(new DepartmentCreateVM()));
        }

        // POST /CompanyAdmin/Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(BuildCreateVM(model));

            // Check duplicate name
            var exists = (await _uow.Departments
                .FindAsync(d => d.Name == model.Name)).Any();
            if (exists)
            {
                ModelState.AddModelError("Name", "يوجد قسم بهذا الاسم مسبقاً");
                return View(BuildCreateVM(model));
            }

            var dept = new Department
            {
                Name      = model.Name,
                ManagerId = string.IsNullOrEmpty(model.ManagerId)
                    ? null : model.ManagerId,
                IsActive  = model.IsActive,
                CreatedBy = User.FindFirst("FullName")?.Value ?? "system",
            };

            await _uow.Departments.AddAsync(dept);
            await _uow.SaveChangesAsync();

            TempData["Success"] = $"تم إنشاء قسم '{model.Name}' بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET /CompanyAdmin/Departments/Edit/id
        public async Task<IActionResult> Edit(Guid id)
        {
            var dept = await _uow.Departments.GetByIdAsync(id);
            if (dept == null) return NotFound();

            var vm = new DepartmentEditVM
            {
                Id        = dept.Id,
                Name      = dept.Name,
                ManagerId = dept.ManagerId,
                IsActive  = dept.IsActive,
            };

            return View(BuildEditVM(vm));
        }

        // POST /CompanyAdmin/Departments/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentEditVM model)
        {
            if (!ModelState.IsValid)
                return View(BuildEditVM(model));

            var dept = await _uow.Departments.GetByIdAsync(model.Id);
            if (dept == null) return NotFound();

            dept.Name      = model.Name;
            dept.ManagerId = string.IsNullOrEmpty(model.ManagerId)
                ? null : model.ManagerId;
            dept.IsActive  = model.IsActive;
            dept.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            dept.UpdatedAt = DateTime.UtcNow;

            _uow.Departments.Update(dept);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات القسم";
            return RedirectToAction(nameof(Index));
        }

        // POST /CompanyAdmin/Departments/ToggleActive/id
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
                ? $"تم تفعيل قسم '{dept.Name}'"
                : $"تم تعطيل قسم '{dept.Name}'";

            return RedirectToAction(nameof(Index));
        }

        // POST /CompanyAdmin/Departments/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var dept = await _uow.Departments.GetByIdAsync(id);
            if (dept == null) return NotFound();

            // Block delete if has employees
            var empCount = (await _uow.Employees
                .FindAsync(e => e.DepartmentId == id)).Count();
            if (empCount > 0)
            {
                TempData["Error"] =
                    $"لا يمكن حذف قسم '{dept.Name}' لأنه يحتوي على {empCount} موظف. انقل الموظفين أولاً.";
                return RedirectToAction(nameof(Index));
            }

            _uow.Departments.Delete(dept);
            await _uow.SaveChangesAsync();

            TempData["Success"] = $"تم حذف قسم '{dept.Name}'";
            return RedirectToAction(nameof(Index));
        }

        // GET /CompanyAdmin/Departments/Employees/id
        public async Task<IActionResult> Employees(Guid id)
        {
            var dept = await _uow.Departments.GetByIdAsync(id);
            if (dept == null) return NotFound();

            var employees = (await _uow.Employees
                .FindAsync(e => e.DepartmentId == id)).ToList();

            ViewBag.Department = dept;
            return View(employees);
        }

        // Private Helpers
        private DepartmentCreateVM BuildCreateVM(DepartmentCreateVM vm)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            // Load active users for this tenant as potential managers
            var tenantUsers = _userManager.Users
                .Where(u => u.TenantId == tenantId && u.IsActive)
                .ToList();

            vm.Managers = tenantUsers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text  = u.FullName ?? u.Email ?? u.Id,
            });

            return vm;
        }

        private DepartmentEditVM BuildEditVM(DepartmentEditVM vm)
        {
            BuildCreateVM(vm);
            return vm;
        }
    }
}

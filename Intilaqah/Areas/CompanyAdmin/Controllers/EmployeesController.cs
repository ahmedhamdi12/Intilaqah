using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class EmployeesController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly INitaqatService _nitaqatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(IUnitOfWork uow, INitaqatService nitaqatService, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _nitaqatService = nitaqatService;
            _userManager = userManager;
        }

        // ── GET: /CompanyAdmin/Employees ──────────────────────────────
        public async Task<IActionResult> Index()
        {
            var employees   = (await _uow.Employees.GetAllAsync()).ToList();
            var departments = (await _uow.Departments.GetAllAsync())
                                .ToDictionary(d => d.Id, d => d.Name);
            var expiringDocs = (await _uow.Documents.GetExpiringAsync(30)).ToList();

            var vm = employees.Select(e => new EmployeeListItemVM
            {
                Id             = e.Id,
                EmployeeCode   = e.EmployeeCode,
                FullNameAr     = e.FullNameAr,
                FullNameEn     = e.FullNameEn,
                JobTitle       = e.JobTitle,
                DepartmentName = e.DepartmentId.HasValue
                    ? departments.GetValueOrDefault(e.DepartmentId.Value, "—")
                    : "—",
                Nationality    = e.Nationality,
                NationalId     = e.NationalId,
                BasicSalary    = e.BasicSalary,
                HireDate       = e.HireDate,
                EmploymentType = e.EmploymentType,
                IsActive       = e.IsActive,
                DocumentsExpiringSoon = expiringDocs
                    .Count(d => d.EntityId == e.Id
                             && d.EntityType == DocumentEntityType.Employee)
            }).ToList();

            ViewBag.TotalCount  = vm.Count;
            ViewBag.SaudiCount  = vm.Count(e => e.Nationality == NationalityType.Saudi);
            ViewBag.ActiveCount = vm.Count(e => e.IsActive);

            return View(vm);
        }

        // ── GET: /CompanyAdmin/Employees/Create ───────────────────────
        public async Task<IActionResult> Create()
        {
            return View(await BuildCreateVM(new EmployeeCreateVM()));
        }

        // ── POST: /CompanyAdmin/Employees/Create ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(await BuildCreateVM(model));

            // Check duplicate Email
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مسجل مسبقاً");
                return View(await BuildCreateVM(model));
            }

            // Check duplicate NationalId
            var exists = (await _uow.Employees
                .FindAsync(e => e.NationalId == model.NationalId))
                .Any();
            if (exists)
            {
                ModelState.AddModelError("NationalId",
                    "رقم الهوية مسجل مسبقاً");
                return View(await BuildCreateVM(model));
            }

            // Generate employee code
            var count = (await _uow.Employees.GetAllAsync()).Count();
            var code  = $"EMP-{(count + 1):D3}";

            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            // Create ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullNameAr,
                TenantId = tenantId == Guid.Empty ? null : tenantId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await _userManager.CreateAsync(user, "Emp@12345");
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Email", "حدث خطأ أثناء إنشاء حساب الدخول للموظف");
                return View(await BuildCreateVM(model));
            }
            await _userManager.AddToRoleAsync(user, DbSeeder.RoleEmployee);

            var employee = new Intilaqah.Models.Employee
            {
                UserId         = user.Id,
                EmployeeCode   = code,
                FullNameAr     = model.FullNameAr,
                FullNameEn     = model.FullNameEn,
                NationalId     = model.NationalId,
                Nationality    = model.Nationality,
                JobTitle       = model.JobTitle,
                DepartmentId   = model.DepartmentId,
                HireDate       = model.HireDate,
                EmploymentType = model.EmploymentType,
                BasicSalary    = model.BasicSalary,
                IsActive       = true,
                CreatedBy      = User.FindFirst("FullName")?.Value ?? "system"
            };

            await _uow.Employees.AddAsync(employee);
            await _uow.SaveChangesAsync();

            // Create initial contract
            var contract = new Contract
            {
                EmployeeId         = employee.Id,
                ContractType       = model.ContractType,
                StartDate          = model.HireDate,
                EndDate            = model.ContractEndDate,
                BasicSalary        = model.BasicSalary,
                HousingAllowance   = model.HousingAllowance,
                TransportAllowance = model.TransportAllowance,
                OtherAllowances    = 0,
                IsActive           = true,
                CreatedBy          = User.FindFirst("FullName")?.Value ?? "system"
            };
            await _uow.Contracts.AddAsync(contract);
            await _uow.SaveChangesAsync();

            // Create bank account if provided
            if (!string.IsNullOrWhiteSpace(model.Iban))
            {
                var bank = new EmployeeBankAccount
                {
                    EmployeeId = employee.Id,
                    BankName   = model.BankName ?? "غير محدد",
                    Iban       = model.Iban,
                    IsActive   = true,
                    CreatedBy  = User.FindFirst("FullName")?.Value ?? "system"
                };
                await _uow.EmployeeBankAccounts.AddAsync(bank);
                await _uow.SaveChangesAsync();
            }

            TempData["Success"] = $"تم إضافة الموظف {model.FullNameAr} بنجاح. كلمة المرور الافتراضية: Emp@12345";

            if (tenantId != Guid.Empty)
                await _nitaqatService.UpdateTenantColorAsync(tenantId);

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /CompanyAdmin/Employees/Edit/id ──────────────────────
        public async Task<IActionResult> Edit(Guid id)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null) return NotFound();

            var contract = await _uow.Contracts.GetActiveContractAsync(id);
            var bankAccount = await _uow.EmployeeBankAccounts.GetActiveByEmployeeAsync(id);

            var vm = new EmployeeEditVM
            {
                Id             = employee.Id,
                EmployeeCode   = employee.EmployeeCode,
                FullNameAr     = employee.FullNameAr,
                FullNameEn     = employee.FullNameEn,
                NationalId     = employee.NationalId,
                Nationality    = employee.Nationality,
                JobTitle       = employee.JobTitle,
                DepartmentId   = employee.DepartmentId,
                HireDate       = employee.HireDate,
                EmploymentType = employee.EmploymentType,
                BasicSalary    = employee.BasicSalary,
                IsActive       = employee.IsActive,
                HousingAllowance   = contract?.HousingAllowance   ?? 0,
                TransportAllowance = contract?.TransportAllowance ?? 0,
                ContractType       = contract?.ContractType       ?? ContractType.Unlimited,
                ContractEndDate    = contract?.EndDate,
                BankName           = bankAccount?.BankName,
                Iban               = bankAccount?.Iban
            };

            return View(await BuildEditVM(vm));
        }

        // ── POST: /CompanyAdmin/Employees/Edit ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditVM model)
        {
            if (!ModelState.IsValid)
                return View(await BuildEditVM(model));

            var employee = await _uow.Employees.GetByIdAsync(model.Id);
            if (employee == null) return NotFound();

            employee.FullNameAr     = model.FullNameAr;
            employee.FullNameEn     = model.FullNameEn;
            employee.NationalId     = model.NationalId;
            employee.Nationality    = model.Nationality;
            employee.JobTitle       = model.JobTitle;
            employee.DepartmentId   = model.DepartmentId;
            employee.HireDate       = model.HireDate;
            employee.EmploymentType = model.EmploymentType;
            employee.BasicSalary    = model.BasicSalary;
            employee.IsActive       = model.IsActive;
            employee.UpdatedBy      = User.FindFirst("FullName")?.Value ?? "system";
            employee.UpdatedAt      = DateTime.UtcNow;

            _uow.Employees.Update(employee);
            
            // Handle bank account
            var bank = await _uow.EmployeeBankAccounts.GetActiveByEmployeeAsync(employee.Id);
            if (!string.IsNullOrWhiteSpace(model.Iban))
            {
                if (bank == null)
                {
                    bank = new EmployeeBankAccount
                    {
                        EmployeeId = employee.Id,
                        BankName   = model.BankName ?? "غير محدد",
                        Iban       = model.Iban,
                        IsActive   = true,
                        CreatedBy  = User.FindFirst("FullName")?.Value ?? "system"
                    };
                    await _uow.EmployeeBankAccounts.AddAsync(bank);
                }
                else
                {
                    bank.BankName  = model.BankName ?? "غير محدد";
                    bank.Iban      = model.Iban;
                    bank.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
                    bank.UpdatedAt = DateTime.UtcNow;
                    _uow.EmployeeBankAccounts.Update(bank);
                }
            }

            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات الموظف";

            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
                await _nitaqatService.UpdateTenantColorAsync(tenantId);

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /CompanyAdmin/Employees/Details/id ───────────────────
        public async Task<IActionResult> Details(Guid id)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null) return NotFound();

            var contract    = await _uow.Contracts.GetActiveContractAsync(id);
            var documents   = (await _uow.Documents
                .GetByEntityAsync(id, DocumentEntityType.Employee)).ToList();
            var departments = (await _uow.Departments.GetAllAsync())
                .ToDictionary(d => d.Id, d => d.Name);
            var shifts = (await _uow.Shifts.GetActiveAsync()).ToList();
            var currentShiftAssignment = await _uow.ShiftAssignments.GetActiveByEmployeeAsync(id);
            var bankAccount = await _uow.EmployeeBankAccounts.GetActiveByEmployeeAsync(id);

            ViewBag.Employee   = employee;
            ViewBag.Contract   = contract;
            ViewBag.Documents  = documents;
            ViewBag.Department = employee.DepartmentId.HasValue
                ? departments.GetValueOrDefault(employee.DepartmentId.Value, "—")
                : "—";
            ViewBag.Shifts       = shifts;
            ViewBag.CurrentShift = currentShiftAssignment?.Shift;
            ViewBag.BankAccount  = bankAccount;

            return View();
        }

        // ── POST: /CompanyAdmin/Employees/ToggleActive ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null) return NotFound();

            employee.IsActive  = !employee.IsActive;
            employee.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
            employee.UpdatedAt = DateTime.UtcNow;

            _uow.Employees.Update(employee);
            await _uow.SaveChangesAsync();

            TempData["Success"] = employee.IsActive
                ? $"تم تفعيل الموظف {employee.FullNameAr}"
                : $"تم تعطيل الموظف {employee.FullNameAr}";

            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
                await _nitaqatService.UpdateTenantColorAsync(tenantId);

            return RedirectToAction(nameof(Index));
        }

        // ── POST: /CompanyAdmin/Employees/Delete ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null) return NotFound();

            employee.IsDeleted = true;
            employee.DeletedBy = User.FindFirst("FullName")?.Value ?? "system";
            employee.DeletedAt = DateTime.UtcNow;

            _uow.Employees.Update(employee);
            await _uow.SaveChangesAsync();

            TempData["Success"] = $"تم حذف الموظف {employee.FullNameAr}";

            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
                await _nitaqatService.UpdateTenantColorAsync(tenantId);

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /CompanyAdmin/Employees/EditBank/id ───────────────────
        public async Task<IActionResult> EditBank(Guid id)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null) return NotFound();

            var bank = await _uow.EmployeeBankAccounts.GetActiveByEmployeeAsync(id);

            var vm = new EmployeeBankVM
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullNameAr,
                BankName = bank?.BankName ?? string.Empty,
                Iban = bank?.Iban ?? string.Empty
            };

            return View(vm);
        }

        // ── POST: /CompanyAdmin/Employees/EditBank ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBank(EmployeeBankVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var bank = await _uow.EmployeeBankAccounts.GetActiveByEmployeeAsync(model.EmployeeId);

            if (bank == null)
            {
                bank = new EmployeeBankAccount
                {
                    EmployeeId = model.EmployeeId,
                    BankName = model.BankName,
                    Iban = model.Iban,
                    IsActive = true,
                    CreatedBy = User.FindFirst("FullName")?.Value ?? "system"
                };
                await _uow.EmployeeBankAccounts.AddAsync(bank);
            }
            else
            {
                bank.BankName = model.BankName;
                bank.Iban = model.Iban;
                bank.UpdatedBy = User.FindFirst("FullName")?.Value ?? "system";
                bank.UpdatedAt = DateTime.UtcNow;
                _uow.EmployeeBankAccounts.Update(bank);
            }

            await _uow.SaveChangesAsync();

            TempData["Success"] = "تم تحديث البيانات البنكية للموظف بنجاح";
            return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
        }

        // ── GET: /CompanyAdmin/Employees/ResetPassword/id ───────────────────
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var employee = await _uow.Employees.GetByIdAsync(id);
            if (employee == null || string.IsNullOrEmpty(employee.UserId)) return NotFound();

            var vm = new Intilaqah.Models.ViewModels.SuperAdmin.ResetPasswordVM
            {
                UserId = employee.UserId,
                UserName = employee.FullNameAr
            };

            return View(vm);
        }

        // ── POST: /CompanyAdmin/Employees/ResetPassword ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Intilaqah.Models.ViewModels.SuperAdmin.ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                var arabicErrors = new Dictionary<string, string> {
                    ["Passwords must be at least 8 characters."] = "كلمة المرور يجب أن تكون 8 أحرف على الأقل",
                    ["Passwords must have at least one digit ('0'-'9')."] = "يجب أن تحتوي على رقم واحد على الأقل",
                    ["Passwords must have at least one non alphanumeric character."] = "يجب أن تحتوي على رمز واحد على الأقل",
                    ["Passwords must have at least one lowercase ('a'-'z')."] = "يجب أن تحتوي على حرف صغير واحد على الأقل",
                    ["Passwords must have at least one uppercase ('A'-'Z')."] = "يجب أن تحتوي على حرف كبير واحد على الأقل"
                };
                foreach (var e in result.Errors)
                {
                    var msg = arabicErrors.GetValueOrDefault(e.Description, e.Description);
                    ModelState.AddModelError("", msg);
                }
                return View(model);
            }

            var employee = (await _uow.Employees.FindAsync(e => e.UserId == user.Id)).FirstOrDefault();

            TempData["Success"] = $"تم تغيير كلمة مرور {user.FullName} بنجاح";
            if (employee != null)
            {
                return RedirectToAction(nameof(Details), new { id = employee.Id });
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Private Helpers ───────────────────────────────────────────
        private async Task<EmployeeCreateVM> BuildCreateVM(EmployeeCreateVM vm)
        {
            var depts = await _uow.Departments.GetActiveAsync();
            vm.Departments = depts.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text  = d.Name
            });
            vm.Nationalities = new[]
            {
                new SelectListItem("سعودي",  NationalityType.Saudi.ToString()),
                new SelectListItem("وافد",   NationalityType.NonSaudi.ToString()),
            };
            vm.EmploymentTypes = new[]
            {
                new SelectListItem("دوام كامل", EmploymentType.FullTime.ToString()),
                new SelectListItem("دوام جزئي", EmploymentType.PartTime.ToString()),
                new SelectListItem("متعاقد",    EmploymentType.Contract.ToString()),
            };
            vm.ContractTypes = new[]
            {
                new SelectListItem("غير محدد المدة", ContractType.Unlimited.ToString()),
                new SelectListItem("محدد المدة",     ContractType.FixedTerm.ToString()),
                new SelectListItem("موسمي",           ContractType.Seasonal.ToString()),
                new SelectListItem("تجريبي",          ContractType.Probation.ToString()),
            };
            return vm;
        }

        private async Task<EmployeeEditVM> BuildEditVM(EmployeeEditVM vm)
        {
            var base_ = await BuildCreateVM(vm);
            return (EmployeeEditVM)base_;
        }
    }
}

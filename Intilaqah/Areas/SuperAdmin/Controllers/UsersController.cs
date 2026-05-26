using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Models.ViewModels.SuperAdmin;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IUnitOfWork _uow;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<AppRole> roleManager, IUnitOfWork uow)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var allUsers = _userManager.Users
                .Where(u => u.TenantId != null)   // exclude SuperAdmin
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            // Load company names
            var tenantIds = allUsers.Where(u => u.TenantId.HasValue)
                .Select(u => u.TenantId!.Value).Distinct().ToList();

            var tenants = new Dictionary<Guid, string>();
            foreach (var tid in tenantIds)
            {
                var t = await _uow.Tenants.GetByIdAsync(tid);
                if (t != null) tenants[tid] = t.Name;
            }

            // Get roles for each user
            var vmList = new List<CompanyUserListItemVM>();
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                vmList.Add(new CompanyUserListItemVM
                {
                    Id = user.Id,
                    FullName = user.FullName ?? user.Email ?? "",
                    Email = user.Email ?? "",
                    RoleName = roles.FirstOrDefault() ?? "—",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    TenantId = user.TenantId,
                    CompanyName = user.TenantId.HasValue
                        ? tenants.GetValueOrDefault(user.TenantId.Value, "—")
                        : "—"
                });
            }

            return View(vmList);
        }

        public async Task<IActionResult> CompanyUsers(Guid tenantId)
        {
            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(tenantId);
            if (tenant == null) return NotFound();

            var users = _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            var vmList = new List<CompanyUserListItemVM>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                vmList.Add(new CompanyUserListItemVM
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "",
                    Email = user.Email ?? "",
                    RoleName = roles.FirstOrDefault() ?? "—",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    TenantId = tenantId,
                    CompanyName = tenant.Name
                });
            }

            ViewBag.Tenant = tenant;
            ViewBag.CurrentCount = users.Count;
            ViewBag.MaxUsers = tenant.Plan?.MaxUsers ?? 0;
            ViewBag.CanAddMore = users.Count < (tenant.Plan?.MaxUsers ?? 0);

            return View(vmList);
        }

        public async Task<IActionResult> Invite(Guid tenantId)
        {
            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(tenantId);
            if (tenant == null) return NotFound();

            var currentCount = _userManager.Users.Count(u => u.TenantId == tenantId);
            var maxUsers = tenant.Plan?.MaxUsers ?? 0;

            if (currentCount >= maxUsers)
            {
                TempData["Error"] = $"وصلت الشركة للحد الأقصى من المستخدمين ({maxUsers} مستخدم). يجب ترقية الباقة أولاً.";
                return RedirectToAction("CompanyUsers", new { tenantId });
            }

            var vm = new InviteUserVM
            {
                TenantId = tenantId,
                CompanyName = tenant.Name,
                CurrentUserCount = currentCount,
                MaxUsers = maxUsers
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Invite(InviteUserVM model)
        {
            // Re-check limit
            var currentCount = _userManager.Users.Count(u => u.TenantId == model.TenantId);
            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(model.TenantId);
            var maxUsers = tenant?.Plan?.MaxUsers ?? 0;

            if (currentCount >= maxUsers)
            {
                ModelState.AddModelError("", "وصلت الشركة للحد الأقصى من المستخدمين.");
                model.CompanyName = tenant?.Name ?? "";
                model.CurrentUserCount = currentCount;
                model.MaxUsers = maxUsers;
                return View(model);
            }

            // Check email uniqueness
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم مسبقاً");
                model.CompanyName = tenant?.Name ?? "";
                model.CurrentUserCount = currentCount;
                model.MaxUsers = maxUsers;
                return View(model);
            }

            // Create user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                TenantId = model.TenantId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var arabicErrors = new Dictionary<string, string> {
                    ["Passwords must be at least 8 characters."] = "كلمة المرور يجب أن تكون 8 أحرف على الأقل",
                    ["Passwords must have at least one digit ('0'-'9')."] = "يجب أن تحتوي على رقم واحد على الأقل",
                    ["Passwords must have at least one non alphanumeric character."] = "يجب أن تحتوي على رمز واحد على الأقل",
                    ["Passwords must have at least one lowercase ('a'-'z')."] = "يجب أن تحتوي على حرف صغير واحد على الأقل",
                    ["Passwords must have at least one uppercase ('A'-'Z')."] = "يجب أن تحتوي على حرف كبير واحد على الأقل"
                };
                foreach (var error in result.Errors)
                {
                    var msg = arabicErrors.GetValueOrDefault(error.Description, error.Description);
                    ModelState.AddModelError("", msg);
                }
                model.CompanyName = tenant?.Name ?? "";
                model.CurrentUserCount = currentCount;
                model.MaxUsers = maxUsers;
                return View(model);
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, model.RoleName);

            TempData["Success"] = $"تم إنشاء حساب {model.FullName} بنجاح";
            return RedirectToAction("CompanyUsers", new { tenantId = model.TenantId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Cannot deactivate yourself
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "لا يمكنك تعطيل حسابك الخاص";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin"))
            {
                TempData["Error"] = "لا يمكن تعطيل حساب المدير الأعلى";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsActive
                ? $"تم تفعيل حساب {user.FullName}"
                : $"تم تعطيل حساب {user.FullName}";

            var referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/SuperAdmin/Users" : referer);
        }

        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return View(new ResetPasswordVM { UserId = userId, UserName = user.FullName ?? user.Email ?? "" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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

            TempData["Success"] = $"تم تغيير كلمة مرور {user.FullName} بنجاح";
            return RedirectToAction("Index");
        }
    }
}

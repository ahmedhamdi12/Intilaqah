using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard();
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "حسابك موقوف. تواصل مع مدير النظام.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                return RedirectToDashboard();
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "تم تأمين الحساب مؤقتاً. حاول بعد قليل.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Forbidden()
        {
            return View();
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole(DbSeeder.RoleSuperAdmin))
            {
                return RedirectToAction("Index", "Dashboard", new { Area = "SuperAdmin" });
            }
            if (User.IsInRole(DbSeeder.RoleCompanyAdmin))
            {
                return RedirectToAction("Index", "Dashboard", new { Area = "CompanyAdmin" });
            }
            if (User.IsInRole(DbSeeder.RoleEmployee))
            {
                return RedirectToAction("Index", "Dashboard", new { Area = "Employee" });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

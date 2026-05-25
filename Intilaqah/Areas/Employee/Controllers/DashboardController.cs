using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UserFullName = User.FindFirst("FullName")?.Value;
            return View();
        }
    }
}

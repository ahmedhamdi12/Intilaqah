using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intilaqah.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class CertificatesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CertificatesController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> SalaryCertificate()
        {
            var userId   = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employee = (await _uow.Employees
                .FindAsync(e => e.UserId == userId && e.IsActive))
                .FirstOrDefault();
            if (employee == null) return Forbid();

            var contract   = await _uow.Contracts
                .GetActiveContractAsync(employee.Id);
            var departments = (await _uow.Departments.GetAllAsync())
                .ToDictionary(d => d.Id, d => d.Name);
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);
            var tenant = await _uow.Tenants.GetByIdWithPlanAsync(tenantId);

            ViewBag.Employee   = employee;
            ViewBag.Contract   = contract;
            ViewBag.Department = employee.DepartmentId.HasValue
                ? departments.GetValueOrDefault(
                    employee.DepartmentId.Value, "—")
                : "—";
            ViewBag.Tenant = tenant;
            ViewBag.IssueDate = DateTime.Today.ToString(
                "dd MMMM yyyy",
                new System.Globalization.CultureInfo("ar-SA"));

            return View();
        }
    }
}

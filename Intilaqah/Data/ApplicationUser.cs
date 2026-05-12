using Microsoft.AspNetCore.Identity;

namespace Intilaqah.Data
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? TenantId { get; set; }
        public string? FullName { get; set; }
        public string Role { get; set; } = "Employee"; // SuperAdmin / CompanyAdmin / Employee
    }
}

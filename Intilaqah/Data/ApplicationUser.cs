using Microsoft.AspNetCore.Identity;

namespace Intilaqah.Data
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? TenantId { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

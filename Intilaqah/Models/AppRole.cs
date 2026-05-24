using Microsoft.AspNetCore.Identity;

namespace Intilaqah.Models
{
    public class AppRole : IdentityRole
    {
        public Guid? TenantId { get; set; }          // null = platform-wide role
        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}

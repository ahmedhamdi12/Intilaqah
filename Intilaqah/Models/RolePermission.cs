namespace Intilaqah.Models
{
    public class RolePermission
    {
        public string RoleId { get; set; } = "";
        public int PermissionId { get; set; }

        public AppRole Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}

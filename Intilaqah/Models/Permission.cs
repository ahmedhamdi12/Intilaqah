namespace Intilaqah.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";       // e.g. "employees.view"
        public string Group { get; set; } = "";      // e.g. "Employees"
        public string Description { get; set; } = "";

        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}

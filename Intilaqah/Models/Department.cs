using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class Department : BaseEntity
    {
        public string Name        { get; set; } = "";
        public string? ManagerId  { get; set; }  // ApplicationUser.Id (optional)
        public bool   IsActive    { get; set; } = true;

        // Navigation
        public ICollection<Employee> Employees { get; set; } = [];
    }
}

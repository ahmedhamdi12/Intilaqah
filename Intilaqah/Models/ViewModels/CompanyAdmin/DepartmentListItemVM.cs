namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class DepartmentListItemVM
    {
        public Guid    Id            { get; set; }
        public string  Name          { get; set; } = "";
        public string  ManagerName   { get; set; } = "—";
        public int     EmployeeCount { get; set; }
        public bool    IsActive      { get; set; }
        public DateTime CreatedAt   { get; set; }
    }
}

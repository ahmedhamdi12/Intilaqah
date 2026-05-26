using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class EmployeeListItemVM
    {
        public Guid           Id             { get; set; }
        public string         EmployeeCode   { get; set; } = "";
        public string         FullNameAr     { get; set; } = "";
        public string         FullNameEn     { get; set; } = "";
        public string         JobTitle       { get; set; } = "";
        public string         DepartmentName { get; set; } = "";
        public NationalityType Nationality   { get; set; }
        public string         NationalId     { get; set; } = "";
        public decimal        BasicSalary    { get; set; }
        public DateTime       HireDate       { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public bool           IsActive       { get; set; }
        public int            DocumentsExpiringSoon { get; set; }
    }
}

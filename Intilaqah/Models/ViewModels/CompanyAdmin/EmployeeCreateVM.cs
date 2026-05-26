using System.ComponentModel.DataAnnotations;
using Intilaqah.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class EmployeeCreateVM
    {
        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [Display(Name = "الاسم الكامل بالعربية")]
        public string FullNameAr { get; set; } = "";

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [Display(Name = "الاسم الكامل بالإنجليزية")]
        public string FullNameEn { get; set; } = "";

        [Required(ErrorMessage = "رقم الهوية مطلوب")]
        [StringLength(10, MinimumLength = 10,
            ErrorMessage = "رقم الهوية يجب أن يكون 10 أرقام")]
        [Display(Name = "رقم الهوية / الإقامة")]
        public string NationalId { get; set; } = "";

        [Required(ErrorMessage = "الجنسية مطلوبة")]
        [Display(Name = "الجنسية")]
        public NationalityType Nationality { get; set; }

        [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
        [Display(Name = "المسمى الوظيفي")]
        public string JobTitle { get; set; } = "";

        [Display(Name = "القسم")]
        public Guid? DepartmentId { get; set; }

        [Required(ErrorMessage = "تاريخ المباشرة مطلوب")]
        [Display(Name = "تاريخ المباشرة")]
        public DateTime HireDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "نوع العقد مطلوب")]
        [Display(Name = "نوع التوظيف")]
        public EmploymentType EmploymentType { get; set; }

        [Required(ErrorMessage = "الراتب الأساسي مطلوب")]
        [Range(1, 999999, ErrorMessage = "الراتب يجب أن يكون أكبر من صفر")]
        [Display(Name = "الراتب الأساسي")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "بدل السكن")]
        [Range(0, 999999, ErrorMessage = "القيمة يجب أن تكون صحيحة")]
        public decimal HousingAllowance { get; set; }

        [Display(Name = "بدل النقل")]
        [Range(0, 999999, ErrorMessage = "القيمة يجب أن تكون صحيحة")]
        public decimal TransportAllowance { get; set; }

        [Display(Name = "نوع العقد")]
        public ContractType ContractType { get; set; } = ContractType.Unlimited;

        [Display(Name = "تاريخ انتهاء العقد")]
        public DateTime? ContractEndDate { get; set; }

        // For dropdowns — populated in controller
        public IEnumerable<SelectListItem> Departments    { get; set; } = [];
        public IEnumerable<SelectListItem> Nationalities  { get; set; } = [];
        public IEnumerable<SelectListItem> EmploymentTypes { get; set; } = [];
        public IEnumerable<SelectListItem> ContractTypes  { get; set; } = [];
    }
}

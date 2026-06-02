using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class ViolationCreateVM
    {
        [Required(ErrorMessage = "الموظف مطلوب")]
        [Display(Name = "الموظف")]
        public Guid EmployeeId { get; set; }

        [Required(ErrorMessage = "المخالفة مطلوبة")]
        [Display(Name = "نوع المخالفة")]
        public Guid ViolationRuleId { get; set; }

        [Required(ErrorMessage = "تاريخ المخالفة مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ المخالفة")]
        public DateTime ViolationDate { get; set; } = DateTime.Today;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; } = [];
        public IEnumerable<SelectListItem> Rules { get; set; } = [];
    }
}

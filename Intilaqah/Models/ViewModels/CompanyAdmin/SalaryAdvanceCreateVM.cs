using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class SalaryAdvanceCreateVM
    {
        [Required(ErrorMessage = "الموظف مطلوب")]
        [Display(Name = "الموظف")]
        public Guid EmployeeId { get; set; }

        [Required(ErrorMessage = "مبلغ السلفة مطلوب")]
        [Range(1, 999999, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        [Display(Name = "إجمالي السلفة")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "القسط الشهري مطلوب")]
        [Range(1, 999999, ErrorMessage = "القسط يجب أن يكون أكبر من صفر")]
        [Display(Name = "القسط الشهري")]
        public decimal MonthlyDeduction { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; } = [];
    }
}

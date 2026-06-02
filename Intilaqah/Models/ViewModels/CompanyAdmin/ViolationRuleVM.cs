using System.ComponentModel.DataAnnotations;
using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class ViolationRuleVM
    {
        [Required(ErrorMessage = "رقم القاعدة مطلوب")]
        [Display(Name = "رقم القاعدة")]
        public int RuleNumber { get; set; }

        [Required(ErrorMessage = "عنوان المخالفة مطلوب")]
        [Display(Name = "عنوان المخالفة")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "درجة المخالفة مطلوبة")]
        [Display(Name = "الدرجة")]
        public ViolationSeverity Severity { get; set; }

        [Required(ErrorMessage = "قيمة الخصم مطلوبة")]
        [Display(Name = "قيمة الخصم (مضاعفات الأجر اليومي)")]
        public decimal DeductionAmount { get; set; }

        [Display(Name = "مفعل؟")]
        public bool IsActive { get; set; } = true;
    }
}

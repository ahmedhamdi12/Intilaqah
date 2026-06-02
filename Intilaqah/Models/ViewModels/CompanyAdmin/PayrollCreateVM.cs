using System.ComponentModel.DataAnnotations;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class PayrollCreateVM
    {
        [Required(ErrorMessage = "الشهر مطلوب")]
        [Range(1, 12, ErrorMessage = "شهر غير صحيح")]
        [Display(Name = "الشهر")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Required(ErrorMessage = "السنة مطلوبة")]
        [Range(2020, 2100)]
        [Display(Name = "السنة")]
        public int Year  { get; set; } = DateTime.Today.Year;

        [Required]
        [Range(1, 31, ErrorMessage = "أيام العمل يجب أن تكون بين 1 و 31")]
        [Display(Name = "أيام العمل الفعلية")]
        public int WorkingDays { get; set; } = 26;
    }
}

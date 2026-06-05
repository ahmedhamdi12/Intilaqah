using System.ComponentModel.DataAnnotations;
using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.Employee
{
    public class LeaveRequestCreateVM
    {
        [Required(ErrorMessage = "نوع الإجازة مطلوب")]
        [Display(Name = "نوع الإجازة")]
        public LeaveType LeaveType { get; set; }

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        [Display(Name = "من تاريخ")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        [Display(Name = "إلى تاريخ")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "سبب الإجازة")]
        [MaxLength(500)]
        public string? Reason { get; set; }

        // Computed — filled in controller
        public int RemainingAnnualDays { get; set; }
    }
}

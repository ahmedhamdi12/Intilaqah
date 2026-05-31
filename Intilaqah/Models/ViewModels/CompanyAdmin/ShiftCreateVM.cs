using System.ComponentModel.DataAnnotations;
using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class ShiftCreateVM
    {
        [Required(ErrorMessage = "اسم الوردية مطلوب")]
        [Display(Name = "اسم الوردية")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "نوع الوردية مطلوب")]
        [Display(Name = "نوع الوردية")]
        public ShiftType ShiftType { get; set; }

        [Required(ErrorMessage = "وقت البداية مطلوب")]
        [Display(Name = "وقت البداية")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "وقت النهاية مطلوب")]
        [Display(Name = "وقت النهاية")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "دقائق السماح (التأخر)")]
        [Range(0, 60)]
        public int GraceMinutes { get; set; } = 5;

        [Display(Name = "الوردية نشطة")]
        public bool IsActive { get; set; } = true;
    }
}

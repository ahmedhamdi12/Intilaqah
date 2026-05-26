using System.ComponentModel.DataAnnotations;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class PlanCreateVM
    {
        public Guid Id { get; set; } // Used for Edit

        [Required(ErrorMessage = "اسم الباقة مطلوب")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "الحد الأقصى للمستخدمين مطلوب")]
        [Range(1, 100, ErrorMessage = "يجب أن يكون بين 1 و 100")]
        public int MaxUsers { get; set; }

        [Required(ErrorMessage = "الحد الأقصى للموظفين مطلوب")]
        [Range(1, 10000, ErrorMessage = "يجب أن يكون بين 1 و 10000")]
        public int MaxEmployees { get; set; }

        public bool IsActive { get; set; } = true;
        
        public bool FeaturePayroll { get; set; } = true;
        public bool FeatureNitaqat { get; set; } = false;
        public bool FeatureAlerts { get; set; } = false;
    }
}

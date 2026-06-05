using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Intilaqah.Models.ViewModels.Employee
{
    public class ProfileEditViewModel
    {
        // Read-only fields for display
        public string FullNameAr { get; set; } = "";
        public string FullNameEn { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string Department { get; set; } = "";

        // Editable fields
        [Display(Name = "رقم الجوال")]
        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [RegularExpression(@"^(05)(5|0|3|6|4|9|1|8|7)([0-9]{7})$", ErrorMessage = "يجب أن يبدأ بـ 05 ويتكون من 10 أرقام")]
        public string? Phone { get; set; }

        public string? CurrentProfilePicturePath { get; set; }

        [Display(Name = "الصورة الشخصية")]
        public IFormFile? ProfilePicture { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class ResetPasswordVM
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(8, ErrorMessage = "يجب أن تكون 8 أحرف على الأقل")]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; } = "";

        [Compare("NewPassword", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = "";
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class InviteUserVM
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [MaxLength(100)]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "الدور الوظيفي مطلوب")]
        [Display(Name = "الدور الوظيفي")]
        public string RoleName { get; set; } = "CompanyAdmin";

        // Set by controller — not shown in form
        public Guid TenantId { get; set; }

        // For display in form
        public string CompanyName { get; set; } = "";
        public int CurrentUserCount { get; set; }
        public int MaxUsers { get; set; }
    }
}

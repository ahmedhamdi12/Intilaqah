using System.ComponentModel.DataAnnotations;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class EmployeeBankVM
    {
        public Guid EmployeeId { get; set; }
        
        public string? EmployeeName { get; set; }

        [Required(ErrorMessage = "اسم البنك مطلوب")]
        [Display(Name = "اسم البنك")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الآيبان (IBAN) مطلوب")]
        [StringLength(24, MinimumLength = 24, ErrorMessage = "رقم الآيبان يجب أن يتكون من 24 حرفاً ورقم")]
        [RegularExpression(@"^SA\d{22}$", ErrorMessage = "رقم الآيبان غير صحيح، يجب أن يبدأ بـ SA ويليه 22 رقماً")]
        [Display(Name = "رقم الآيبان (IBAN)")]
        public string Iban { get; set; } = string.Empty;
    }
}

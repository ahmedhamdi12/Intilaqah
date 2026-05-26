using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class TenantCreateVM
    {
        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم السجل التجاري مطلوب")]
        [MaxLength(20)]
        public string CommercialRegNo { get; set; }

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "يجب اختيار باقة")]
        public Guid PlanId { get; set; }

        [Required(ErrorMessage = "تاريخ انتهاء العقد مطلوب")]
        public DateTime? ContractEndDate { get; set; }

        public NitaqatColor NitaqatColor { get; set; } = NitaqatColor.Green;

        public IEnumerable<SelectListItem> AvailablePlans { get; set; } = new List<SelectListItem>();
    }
}

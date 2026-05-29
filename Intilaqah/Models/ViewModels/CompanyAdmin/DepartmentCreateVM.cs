using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class DepartmentCreateVM
    {
        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [MaxLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
        [Display(Name = "اسم القسم")]
        public string Name { get; set; } = "";

        [Display(Name = "مدير القسم")]
        public string? ManagerId { get; set; }

        [Display(Name = "حالة القسم")]
        public bool IsActive { get; set; } = true;

        // For dropdown — populated in controller
        public IEnumerable<SelectListItem> Managers { get; set; } = [];
    }
}

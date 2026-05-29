using System.ComponentModel.DataAnnotations;
using Intilaqah.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class DocumentUploadVM
    {
        // Who this document belongs to
        public Guid               EntityId   { get; set; }
        public DocumentEntityType EntityType { get; set; }

        // For display
        public string EntityName { get; set; } = "";

        [Required(ErrorMessage = "نوع الوثيقة مطلوب")]
        [Display(Name = "نوع الوثيقة")]
        public string DocType { get; set; } = "";

        [Display(Name = "تاريخ الانتهاء")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "الملف")]
        public IFormFile? File { get; set; }

        // For dropdown
        public IEnumerable<SelectListItem> DocTypeOptions { get; set; } = [];
    }
}

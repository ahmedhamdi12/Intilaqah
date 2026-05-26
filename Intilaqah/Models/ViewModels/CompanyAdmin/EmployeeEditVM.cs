using System.ComponentModel.DataAnnotations;
using Intilaqah.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class EmployeeEditVM : EmployeeCreateVM
    {
        public Guid   Id           { get; set; }
        public string EmployeeCode { get; set; } = "";
        public bool   IsActive     { get; set; } = true;
    }
}

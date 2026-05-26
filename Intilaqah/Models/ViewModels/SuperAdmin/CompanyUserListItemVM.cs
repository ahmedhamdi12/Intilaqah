using System;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class CompanyUserListItemVM
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? TenantId { get; set; }
        public string CompanyName { get; set; } = "";
    }
}

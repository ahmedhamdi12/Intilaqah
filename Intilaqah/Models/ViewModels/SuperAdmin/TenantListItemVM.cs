using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class TenantListItemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CommercialRegNo { get; set; }
        public string? Phone { get; set; }
        public string PlanName { get; set; }
        public int EmployeeCount { get; set; }
        public TenantStatus Status { get; set; }
        public NitaqatColor NitaqatColor { get; set; }
        public DateTime ContractEndDate { get; set; }
        public int DaysUntilExpiry => (ContractEndDate.Date - DateTime.UtcNow.Date).Days;
        public bool IsExpiringSoon => DaysUntilExpiry <= 30;
    }
}

using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class SuperAdminDashboardVM
    {
        public int TotalCompanies      { get; set; }
        public int ActiveCompanies     { get; set; }
        public int FrozenCompanies     { get; set; }
        public int SuspendedCompanies  { get; set; }
        public int TotalEmployees      { get; set; }
        public int ExpiringContracts30 { get; set; }
        public int ExpiringContracts7  { get; set; }
        public int TotalPlans          { get; set; }

        public int NitaqatPlatinum { get; set; }
        public int NitaqatGreen    { get; set; }
        public int NitaqatYellow   { get; set; }
        public int NitaqatRed      { get; set; }

        public List<ExpiringContractVM>  ExpiringContracts { get; set; } = new();
        public List<RecentCompanyVM>     RecentCompanies   { get; set; } = new();
        public List<PlanUtilizationVM>   PlanUtilizations  { get; set; } = new();
    }

    public class ExpiringContractVM
    {
        public Guid     Id              { get; set; }
        public string   CompanyName     { get; set; } = "";
        public string   PlanName        { get; set; } = "";
        public DateTime ContractEndDate { get; set; }
        public int      DaysRemaining   { get; set; }
        public string   UrgencyClass    { get; set; } = "";
    }

    public class RecentCompanyVM
    {
        public Guid         Id            { get; set; }
        public string       Name          { get; set; } = "";
        public string       PlanName      { get; set; } = "";
        public TenantStatus Status        { get; set; }
        public NitaqatColor NitaqatColor  { get; set; }
        public DateTime     CreatedAt     { get; set; }
        public int          EmployeeCount { get; set; }
    }

    public class PlanUtilizationVM
    {
        public string Name         { get; set; } = "";
        public int    TenantsCount { get; set; }
        public int    MaxEmployees { get; set; }
        public bool   IsActive     { get; set; }
    }
}

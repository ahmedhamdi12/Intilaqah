using System;
using Intilaqah.Models.Integration;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class IntegrationSettingsVM
    {
        public Guid               TenantId    { get; set; }

        // Qiwa
        public bool    QiwaEnabled   { get; set; }
        public string? QiwaApiKey    { get; set; }
        public string? QiwaClientId  { get; set; }
        public string? QiwaExternalId { get; set; }
        public DateTime? QiwaLastSync { get; set; }
        public string? QiwaLastStatus { get; set; }

        // Mudad
        public bool    MudadEnabled    { get; set; }
        public string? MudadApiKey     { get; set; }
        public string? MudadClientId   { get; set; }
        public string? MudadExternalId { get; set; }
        public DateTime? MudadLastSync  { get; set; }
        public string? MudadLastStatus  { get; set; }
    }
}

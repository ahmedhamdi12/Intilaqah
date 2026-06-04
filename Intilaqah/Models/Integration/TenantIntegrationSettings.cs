using System;
using Intilaqah.Models.Base;

namespace Intilaqah.Models.Integration
{
    public class TenantIntegrationSettings : BaseEntity
    {
        public IntegrationProvider Provider      { get; set; }
        public bool                IsEnabled     { get; set; } = false;
        public string?             ApiKey        { get; set; }
        // TODO: encrypt at rest using IDataProtector before production
        public string?             ApiSecret     { get; set; }
        public string?             ClientId      { get; set; }
        public string?             ExternalId    { get; set; }
        // e.g. Qiwa establishment ID / Mudad company ID
        public DateTime?           LastSyncAt    { get; set; }
        public string?             LastSyncStatus { get; set; }
        public string?             Notes         { get; set; }
    }
}

using System;

namespace Intilaqah.Models.Integration
{
    public class IntegrationLog
    {
        public Guid                Id           { get; set; } = Guid.NewGuid();
        public Guid?               TenantId     { get; set; }
        public IntegrationProvider Provider     { get; set; }
        public string              Operation    { get; set; } = "";
        // e.g. "SyncEmployees", "GetNitaqat", "SubmitWPS"
        public string?             RequestBody  { get; set; }   // JSON
        public string?             ResponseBody { get; set; }   // JSON
        public IntegrationStatus   Status       { get; set; }
        public int                 HttpStatusCode { get; set; }
        public string?             ErrorMessage { get; set; }
        public int                 RetryCount   { get; set; } = 0;
        public Guid?               RetryOf      { get; set; }
        // points to original log entry if this is a retry
        public DateTime?           NextRetryAt  { get; set; }
        public long                DurationMs   { get; set; }
        // how long the API call took
        public DateTime            CreatedAt    { get; set; } = DateTime.UtcNow;
    }
}

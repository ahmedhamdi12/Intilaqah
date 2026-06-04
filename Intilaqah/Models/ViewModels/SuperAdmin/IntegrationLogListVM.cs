using System;
using Intilaqah.Models.Integration;

namespace Intilaqah.Models.ViewModels.SuperAdmin
{
    public class IntegrationLogListVM
    {
        public Guid                Id           { get; set; }
        public string              TenantName   { get; set; } = "";
        public IntegrationProvider Provider     { get; set; }
        public string              Operation    { get; set; } = "";
        public IntegrationStatus   Status       { get; set; }
        public int                 HttpStatus   { get; set; }
        public string?             ErrorMessage { get; set; }
        public int                 RetryCount   { get; set; }
        public long                DurationMs   { get; set; }
        public DateTime            CreatedAt    { get; set; }
    }
}

using System;
using System.Threading.Tasks;
using Intilaqah.Infrastructure.Integrations.DTOs;

namespace Intilaqah.Infrastructure.Integrations.Interfaces
{
    public interface IMudadService
    {
        /// <summary>
        /// Submit WPS payroll file to Mudad for processing.
        /// </summary>
        Task<IntegrationResult> SubmitWpsAsync(
            Guid tenantId, Guid payrollRunId);

        /// <summary>
        /// Get WPS submission status from Mudad.
        /// </summary>
        Task<IntegrationResult<string>> GetWpsStatusAsync(
            Guid tenantId, string submissionId);

        /// <summary>
        /// Get company registration details from Mudad.
        /// </summary>
        Task<IntegrationResult<MudadCompanyInfo>> GetCompanyInfoAsync(
            Guid tenantId);
    }
}

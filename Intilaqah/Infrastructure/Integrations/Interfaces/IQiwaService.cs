using System;
using System.Threading.Tasks;
using Intilaqah.Infrastructure.Integrations.DTOs;

namespace Intilaqah.Infrastructure.Integrations.Interfaces
{
    public interface IQiwaService
    {
        /// <summary>
        /// Sync employee data to Qiwa platform.
        /// Called after employee create/update.
        /// </summary>
        Task<IntegrationResult> SyncEmployeeAsync(
            Guid tenantId, Guid employeeId);

        /// <summary>
        /// Get Nitaqat color for a tenant from Qiwa.
        /// </summary>
        Task<IntegrationResult<string>> GetNitaqatColorAsync(Guid tenantId);

        /// <summary>
        /// Verify employee work permit status on Qiwa.
        /// </summary>
        Task<IntegrationResult<bool>> VerifyWorkPermitAsync(
            Guid tenantId, string nationalId);
    }
}

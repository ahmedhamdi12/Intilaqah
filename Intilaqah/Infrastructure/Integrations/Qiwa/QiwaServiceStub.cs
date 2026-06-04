using System;
using System.Threading.Tasks;
using Intilaqah.Data;
using Intilaqah.Infrastructure.Integrations.DTOs;
using Intilaqah.Infrastructure.Integrations.Interfaces;
using Intilaqah.Models;
using Intilaqah.Models.Integration;
using System.Diagnostics;

namespace Intilaqah.Infrastructure.Integrations.Qiwa
{
    /// <summary>
    /// STUB implementation — returns mock data.
    /// Replace with real HTTP calls when Qiwa API credentials are available.
    /// Real implementation: QiwaService.cs (to be created in Sprint 5A-impl)
    /// </summary>
    public class QiwaServiceStub : IQiwaService
    {
        private readonly ApplicationDbContext _context;

        public QiwaServiceStub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IntegrationResult> SyncEmployeeAsync(
            Guid tenantId, Guid employeeId)
        {
            // STUB — replace with real HTTP call in Sprint 5A-impl
            var log = await LogAsync(
                tenantId,
                IntegrationProvider.Qiwa,
                "SyncEmployee",
                $"{{\"employeeId\":\"{employeeId}\"}}",
                "{\"status\":\"synced\",\"stub\":true}",
                IntegrationStatus.Success,
                200, 45);

            return IntegrationResult.Success(log.Id);
        }

        public async Task<IntegrationResult<string>> GetNitaqatColorAsync(
            Guid tenantId)
        {
            // STUB — returns mock "Green" color
            var log = await LogAsync(
                tenantId,
                IntegrationProvider.Qiwa,
                "GetNitaqatColor",
                $"{{\"tenantId\":\"{tenantId}\"}}",
                "{\"color\":\"Green\",\"stub\":true}",
                IntegrationStatus.Success,
                200, 120);

            return IntegrationResult<string>.Success("Green", log.Id);
        }

        public async Task<IntegrationResult<bool>> VerifyWorkPermitAsync(
            Guid tenantId, string nationalId)
        {
            // STUB — always returns valid
            var log = await LogAsync(
                tenantId,
                IntegrationProvider.Qiwa,
                "VerifyWorkPermit",
                $"{{\"nationalId\":\"{nationalId}\"}}",
                "{\"valid\":true,\"stub\":true}",
                IntegrationStatus.Success,
                200, 80);

            return IntegrationResult<bool>.Success(true, log.Id);
        }

        private async Task<IntegrationLog> LogAsync(
            Guid tenantId, IntegrationProvider provider,
            string operation, string? request, string? response,
            IntegrationStatus status, int httpStatus, long durationMs)
        {
            var log = new IntegrationLog
            {
                TenantId       = tenantId,
                Provider       = provider,
                Operation      = operation,
                RequestBody    = request,
                ResponseBody   = response,
                Status         = status,
                HttpStatusCode = httpStatus,
                DurationMs     = durationMs,
                CreatedAt      = DateTime.UtcNow,
            };
            _context.IntegrationLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }
    }
}

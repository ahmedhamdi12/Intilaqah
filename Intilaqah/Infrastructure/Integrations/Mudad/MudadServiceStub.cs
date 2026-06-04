using System;
using System.Threading.Tasks;
using Intilaqah.Data;
using Intilaqah.Infrastructure.Integrations.DTOs;
using Intilaqah.Infrastructure.Integrations.Interfaces;
using Intilaqah.Models;
using Intilaqah.Models.Integration;

namespace Intilaqah.Infrastructure.Integrations.Mudad
{
    /// <summary>
    /// STUB implementation — returns mock data.
    /// Replace with real HTTP calls when Mudad API credentials are available.
    /// Real implementation: MudadService.cs (to be created in Sprint 5A-impl)
    /// </summary>
    public class MudadServiceStub : IMudadService
    {
        private readonly ApplicationDbContext _context;

        public MudadServiceStub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IntegrationResult> SubmitWpsAsync(
            Guid tenantId, Guid payrollRunId)
        {
            // STUB — replace with real WPS file submission in Sprint 5A-impl
            var log = await LogAsync(
                tenantId,
                IntegrationProvider.Mudad,
                "SubmitWPS",
                $"{{\"payrollRunId\":\"{payrollRunId}\"}}",
                "{\"submissionId\":\"WPS-STUB-001\",\"status\":\"received\"}",
                IntegrationStatus.Success,
                200, 340);

            return IntegrationResult.Success(log.Id);
        }

        public async Task<IntegrationResult<string>> GetWpsStatusAsync(
            Guid tenantId, string submissionId)
        {
            // STUB — always returns processed
            var log = await LogAsync(
                tenantId,
                IntegrationProvider.Mudad,
                "GetWPSStatus",
                $"{{\"submissionId\":\"{submissionId}\"}}",
                "{\"status\":\"processed\",\"stub\":true}",
                IntegrationStatus.Success,
                200, 90);

            return IntegrationResult<string>.Success("processed", log.Id);
        }

        public async Task<IntegrationResult<MudadCompanyInfo>> GetCompanyInfoAsync(
            Guid tenantId)
        {
            // STUB — returns mock company info
            var log = await LogAsync(
                tenantId,
                IntegrationProvider.Mudad,
                "GetCompanyInfo",
                $"{{\"tenantId\":\"{tenantId}\"}}",
                "{\"nitaqatColor\":\"Green\",\"stub\":true}",
                IntegrationStatus.Success,
                200, 150);

            return IntegrationResult<MudadCompanyInfo>.Success(
                new MudadCompanyInfo
                {
                    CompanyName    = "Stub Company",
                    NitaqatColor   = "Green",
                    EmployeeCount  = 0,
                }, log.Id);
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

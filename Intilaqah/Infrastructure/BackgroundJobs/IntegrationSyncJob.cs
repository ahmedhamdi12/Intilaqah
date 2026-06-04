using System;
using System.Linq;
using System.Threading.Tasks;
using Intilaqah.Data;
using Intilaqah.Models.Integration;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Retries failed integration calls.
    /// Runs every 30 minutes via Hangfire.
    /// </summary>
    public class IntegrationSyncJob
    {
        private readonly ApplicationDbContext _context;

        public IntegrationSyncJob(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RunAsync()
        {
            var now = DateTime.UtcNow;

            // Find failed logs eligible for retry.
            // We ensure we don't pick up logs that have already been retried
            // by checking if there's any log that has RetryOf == log.Id
            var failedLogs = await _context.IntegrationLogs
                .Where(l => l.Status    == IntegrationStatus.Failed
                         && l.RetryCount < 3
                         && l.NextRetryAt.HasValue
                         && l.NextRetryAt.Value <= now
                         && !_context.IntegrationLogs.Any(r => r.RetryOf == l.Id))
                .ToListAsync();

            foreach (var log in failedLogs)
            {
                // ADJUSTMENT: Do not update existing IntegrationLog records during retries.
                // Create new retry entries only and keep original failed logs unchanged.

                // Create retry log entry
                var retryLog = new IntegrationLog
                {
                    TenantId       = log.TenantId,
                    Provider       = log.Provider,
                    Operation      = log.Operation,
                    RequestBody    = log.RequestBody,
                    Status         = IntegrationStatus.Pending,
                    RetryCount     = log.RetryCount + 1,
                    RetryOf        = log.Id,
                    CreatedAt      = now,
                };
                _context.IntegrationLogs.Add(retryLog);

                // TODO: In Sprint 5A-impl, actually re-call the service here
                // For now just log the retry attempt as pending
            }

            await _context.SaveChangesAsync();
        }
    }
}

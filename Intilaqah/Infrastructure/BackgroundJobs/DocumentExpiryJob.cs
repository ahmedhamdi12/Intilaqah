using Intilaqah.Services;

namespace Intilaqah.Infrastructure.BackgroundJobs
{
    public class DocumentExpiryJob
    {
        private readonly IDocumentAlertService _alertService;

        public DocumentExpiryJob(IDocumentAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task RunAsync()
        {
            await _alertService.SendExpiryAlertsAsync();
        }
    }
}

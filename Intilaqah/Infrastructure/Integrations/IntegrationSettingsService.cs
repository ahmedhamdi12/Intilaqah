using System;
using System.Threading.Tasks;
using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Models.Integration;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Infrastructure.Integrations
{
    public interface IIntegrationSettingsService
    {
        Task<TenantIntegrationSettings?> GetAsync(
            Guid tenantId, IntegrationProvider provider);
        Task SaveAsync(TenantIntegrationSettings settings);
        Task<bool> IsEnabledAsync(
            Guid tenantId, IntegrationProvider provider);
    }

    public class IntegrationSettingsService : IIntegrationSettingsService
    {
        private readonly ApplicationDbContext _context;

        public IntegrationSettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TenantIntegrationSettings?> GetAsync(
            Guid tenantId, IntegrationProvider provider)
            => await _context.TenantIntegrationSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s =>
                    s.TenantId == tenantId
                    && s.Provider == provider
                    && !s.IsDeleted);

        public async Task SaveAsync(TenantIntegrationSettings settings)
        {
            var existing = await GetAsync(
                settings.TenantId, settings.Provider);

            if (existing == null)
            {
                // Assign Id and timestamps if required, but DbContext handles it 
                _context.TenantIntegrationSettings.Add(settings);
            }
            else
            {
                existing.IsEnabled    = settings.IsEnabled;
                // Only update keys if they are not masked (the UI should handle this or backend should ignore masked updates, 
                // but since controller will handle masked input by not updating them, we can assign directly)
                // Actually the controller will either pass the real string or null/empty if unchanged, wait no.
                // The prompt adjustment said "Do not return API keys back to the UI after saving. Display masked values only."
                // So if the UI sends back the masked value (e.g. "••••••••••••••••") or empty, we shouldn't overwrite it with the masked string.
                // I will add a check here to avoid overwriting with a masked string.
                if (!string.IsNullOrEmpty(settings.ApiKey) && settings.ApiKey != "••••••••••••••••")
                {
                    existing.ApiKey = settings.ApiKey;
                }
                
                if (!string.IsNullOrEmpty(settings.ApiSecret) && settings.ApiSecret != "••••••••••••••••")
                {
                    existing.ApiSecret = settings.ApiSecret;
                }

                existing.ClientId     = settings.ClientId;
                existing.ExternalId   = settings.ExternalId;
                existing.Notes        = settings.Notes;
                // Existing entity will be updated by DbContext ChangeTracker for UpdatedAt/UpdatedBy
                _context.TenantIntegrationSettings.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEnabledAsync(
            Guid tenantId, IntegrationProvider provider)
        {
            var settings = await GetAsync(tenantId, provider);
            return settings?.IsEnabled == true
                && !string.IsNullOrEmpty(settings.ApiKey);
        }
    }
}

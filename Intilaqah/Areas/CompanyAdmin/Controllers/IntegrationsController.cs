using System;
using System.Threading.Tasks;
using Intilaqah.Infrastructure.Integrations;
using Intilaqah.Models;
using Intilaqah.Models.Integration;
using Intilaqah.Models.ViewModels.CompanyAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intilaqah.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Authorize(Roles = "CompanyAdmin")]
    public class IntegrationsController : Controller
    {
        private readonly IUnitOfWork                   _uow;
        private readonly IIntegrationSettingsService   _settingsService;

        public IntegrationsController(
            IUnitOfWork uow,
            IIntegrationSettingsService settingsService)
        {
            _uow             = uow;
            _settingsService = settingsService;
        }

        // GET /CompanyAdmin/Integrations
        public async Task<IActionResult> Index()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            var qiwa  = await _settingsService
                .GetAsync(tenantId, IntegrationProvider.Qiwa);
            var mudad = await _settingsService
                .GetAsync(tenantId, IntegrationProvider.Mudad);

            // ADJUSTMENT: Display masked values only. Do not return API keys back to UI.
            string? MaskApiKey(string? key) => string.IsNullOrEmpty(key) ? null : "••••••••••••••••";

            var vm = new IntegrationSettingsVM
            {
                TenantId = tenantId,

                QiwaEnabled    = qiwa?.IsEnabled    ?? false,
                QiwaApiKey     = MaskApiKey(qiwa?.ApiKey),
                QiwaClientId   = qiwa?.ClientId,
                QiwaExternalId = qiwa?.ExternalId,
                QiwaLastSync   = qiwa?.LastSyncAt,
                QiwaLastStatus = qiwa?.LastSyncStatus,

                MudadEnabled    = mudad?.IsEnabled    ?? false,
                MudadApiKey     = MaskApiKey(mudad?.ApiKey),
                MudadClientId   = mudad?.ClientId,
                MudadExternalId = mudad?.ExternalId,
                MudadLastSync   = mudad?.LastSyncAt,
                MudadLastStatus = mudad?.LastSyncStatus,
            };

            return View(vm);
        }

        // POST /CompanyAdmin/Integrations/SaveQiwa
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveQiwa(IntegrationSettingsVM model)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            var settings = new TenantIntegrationSettings
            {
                TenantId    = tenantId,
                Provider    = IntegrationProvider.Qiwa,
                IsEnabled   = model.QiwaEnabled,
                ApiKey      = model.QiwaApiKey,
                ClientId    = model.QiwaClientId,
                ExternalId  = model.QiwaExternalId,
                CreatedBy   = User.FindFirst("FullName")?.Value ?? "system",
            };

            await _settingsService.SaveAsync(settings);

            TempData["Success"] = "تم حفظ إعدادات قوى";
            return RedirectToAction(nameof(Index));
        }

        // POST /CompanyAdmin/Integrations/SaveMudad
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMudad(IntegrationSettingsVM model)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            Guid.TryParse(tenantIdClaim, out var tenantId);

            var settings = new TenantIntegrationSettings
            {
                TenantId    = tenantId,
                Provider    = IntegrationProvider.Mudad,
                IsEnabled   = model.MudadEnabled,
                ApiKey      = model.MudadApiKey,
                ClientId    = model.MudadClientId,
                ExternalId  = model.MudadExternalId,
                CreatedBy   = User.FindFirst("FullName")?.Value ?? "system",
            };

            await _settingsService.SaveAsync(settings);

            TempData["Success"] = "تم حفظ إعدادات مدد";
            return RedirectToAction(nameof(Index));
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Intilaqah.Data;
using Intilaqah.Models.Integration;
using Intilaqah.Models.ViewModels.SuperAdmin;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class IntegrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork          _uow;

        public IntegrationsController(
            ApplicationDbContext context, IUnitOfWork uow)
        {
            _context = context;
            _uow     = uow;
        }

        // GET /SuperAdmin/Integrations
        // Integration logs overview
        public async Task<IActionResult> Index(
            string? provider = null,
            string? status   = null,
            int     page     = 1)
        {
            var query = _context.IntegrationLogs.AsQueryable();

            if (!string.IsNullOrEmpty(provider)
                && Enum.TryParse<IntegrationProvider>(
                    provider, out var providerEnum))
                query = query.Where(l => l.Provider == providerEnum);

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<IntegrationStatus>(
                    status, out var statusEnum))
                query = query.Where(l => l.Status == statusEnum);

            var total = await query.CountAsync();
            var logs  = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * 50)
                .Take(50)
                .ToListAsync();

            // Load tenant names
            var tenantIds = logs
                .Where(l => l.TenantId.HasValue)
                .Select(l => l.TenantId!.Value)
                .Distinct().ToList();

            var tenants = new Dictionary<Guid, string>();
            foreach (var tid in tenantIds)
            {
                var t = await _uow.Tenants.GetByIdAsync(tid);
                if (t != null) tenants[tid] = t.Name;
            }

            var vm = logs.Select(l => new IntegrationLogListVM
            {
                Id           = l.Id,
                TenantName   = l.TenantId.HasValue
                    ? tenants.GetValueOrDefault(l.TenantId.Value, "—")
                    : "SuperAdmin",
                Provider     = l.Provider,
                Operation    = l.Operation,
                Status       = l.Status,
                HttpStatus   = l.HttpStatusCode,
                ErrorMessage = l.ErrorMessage,
                RetryCount   = l.RetryCount,
                DurationMs   = l.DurationMs,
                CreatedAt    = l.CreatedAt,
            }).ToList();

            ViewBag.TotalCount    = total;
            ViewBag.CurrentPage   = page;
            ViewBag.TotalPages    = (int)Math.Ceiling(total / 50.0);
            ViewBag.ProviderFilter = provider;
            ViewBag.StatusFilter   = status;

            // Quick stats
            ViewBag.SuccessCount = await _context.IntegrationLogs
                .CountAsync(l => l.Status == IntegrationStatus.Success);
            ViewBag.FailedCount  = await _context.IntegrationLogs
                .CountAsync(l => l.Status == IntegrationStatus.Failed);
            ViewBag.PendingCount = await _context.IntegrationLogs
                .CountAsync(l => l.Status == IntegrationStatus.Pending
                              || l.Status == IntegrationStatus.Retrying);

            return View(vm);
        }
    }
}

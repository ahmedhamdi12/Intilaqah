using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Infrastructure.Audit
{
    public class AuditService : IAuditService
    {
        private readonly IServiceScopeFactory  _scopeFactory;
        private readonly ITenantResolver       _tenantResolver;
        private readonly IHttpContextAccessor  _httpAccessor;

        public AuditService(
            IServiceScopeFactory  scopeFactory,
            ITenantResolver       tenantResolver,
            IHttpContextAccessor  httpAccessor)
        {
            _scopeFactory   = scopeFactory;
            _tenantResolver = tenantResolver;
            _httpAccessor   = httpAccessor;
        }

        public async Task LogAsync(
            string  action,
            string  entityName,
            string? entityId  = null,
            string? oldValues = null,
            string? newValues = null,
            Guid?   tenantId  = null)
        {
            try
            {
                var userId   = _tenantResolver.GetCurrentUserId();
                var userName = _httpAccessor.HttpContext?.User
                    .FindFirst("FullName")?.Value;
                var ip = _httpAccessor.HttpContext?.Connection
                    .RemoteIpAddress?.ToString();

                var log = new AuditLog
                {
                    TenantId   = tenantId ?? _tenantResolver.GetTenantId(),
                    UserId     = userId,
                    UserName   = userName,
                    Action     = action,
                    EntityName = entityName,
                    EntityId   = entityId,
                    OldValues  = oldValues,
                    NewValues  = newValues,
                    IpAddress  = ip,
                    CreatedAt  = DateTime.UtcNow,
                };

                // Use a fresh scope + DbContext to avoid shared-state conflicts
                // with the UnitOfWork's DbContext that triggered this audit
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.AuditLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log to console so we can debug — never break the main flow
                System.Diagnostics.Debug.WriteLine($"[AuditService ERROR] {ex.Message}");
                Console.WriteLine($"[AuditService ERROR] {ex.Message}");
            }
        }
    }
}

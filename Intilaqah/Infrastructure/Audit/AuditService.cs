using Intilaqah.Models;
using Intilaqah.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Intilaqah.Infrastructure.Audit
{
    public class AuditService : IAuditService
    {
        private readonly string _connectionString;
        private readonly ITenantResolver       _tenantResolver;
        private readonly IHttpContextAccessor  _httpAccessor;

        public AuditService(
            IConfiguration        configuration,
            ITenantResolver       tenantResolver,
            IHttpContextAccessor  httpAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _tenantResolver   = tenantResolver;
            _httpAccessor     = httpAccessor;
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

                var id        = Guid.NewGuid();
                var createdAt = DateTime.UtcNow;
                var tenant    = tenantId ?? _tenantResolver.GetTenantId();

                // Direct ADO.NET — completely independent from EF DbContext
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
                    INSERT INTO AuditLogs 
                        (Id, TenantId, UserId, UserName, [Action], EntityName, EntityId, OldValues, NewValues, IpAddress, CreatedAt)
                    VALUES 
                        (@Id, @TenantId, @UserId, @UserName, @Action, @EntityName, @EntityId, @OldValues, @NewValues, @IpAddress, @CreatedAt)";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id",         id);
                cmd.Parameters.AddWithValue("@TenantId",   (object?)tenant ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserId",     (object?)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserName",   (object?)userName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Action",     action);
                cmd.Parameters.AddWithValue("@EntityName", entityName);
                cmd.Parameters.AddWithValue("@EntityId",   (object?)entityId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OldValues",  (object?)oldValues ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NewValues",  (object?)newValues ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IpAddress",  (object?)ip ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt",  createdAt);

                await cmd.ExecuteNonQueryAsync();

                Console.WriteLine($"[AuditLog] ✅ {action} on {entityName} (ID: {entityId}) by {userName}");
            }
            catch (Exception ex)
            {
                // Never let audit failure break the main flow, but log it
                Console.WriteLine($"[AuditService ERROR] ❌ {ex.Message}");
                Console.WriteLine($"[AuditService ERROR] Stack: {ex.StackTrace}");
            }
        }
    }
}

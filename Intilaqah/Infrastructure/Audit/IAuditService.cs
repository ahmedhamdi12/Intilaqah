namespace Intilaqah.Infrastructure.Audit
{
    public interface IAuditService
    {
        Task LogAsync(
            string  action,
            string  entityName,
            string? entityId    = null,
            string? oldValues   = null,
            string? newValues   = null,
            Guid?   tenantId    = null);
    }
}

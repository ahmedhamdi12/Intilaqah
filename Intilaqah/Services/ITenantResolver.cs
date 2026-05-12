namespace Intilaqah.Services
{
    public interface ITenantResolver
    {
        Guid? GetTenantId();
        string? GetCurrentUserId();
    }
}

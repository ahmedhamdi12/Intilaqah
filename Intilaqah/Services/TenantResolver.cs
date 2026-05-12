
namespace Intilaqah.Services
{
    public class TenantResolver : ITenantResolver
    {
        private readonly IHttpContextAccessor _accessor;

        public TenantResolver(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid? GetTenantId()
        {
            var claim = _accessor.HttpContext?.User?.FindFirst("TenantId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }

        public string? GetCurrentUserId()
            => _accessor.HttpContext?.User?.FindFirst(
                   System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}

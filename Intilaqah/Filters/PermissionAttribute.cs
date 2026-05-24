using Intilaqah.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Intilaqah.Filters
{
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var repo = context.HttpContext.RequestServices
                .GetRequiredService<IPermissionRepository>();

            var roleId = context.HttpContext.User.FindFirst("RoleId")?.Value;
            if (roleId == null || !await repo.HasPermissionAsync(roleId, _permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

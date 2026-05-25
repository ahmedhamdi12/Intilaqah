using System.Security.Claims;
using Intilaqah.Data;
using Intilaqah.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Intilaqah.Infrastructure.Security
{
    public class CustomClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, AppRole>
    {
        public CustomClaimsFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<AppRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (user.TenantId.HasValue)
            {
                identity.AddClaim(new Claim("TenantId", user.TenantId.Value.ToString()));
            }

            var roles = await UserManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await RoleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    identity.AddClaim(new Claim("RoleId", role.Id));
                    identity.AddClaim(new Claim("RoleName", role.Name!));
                }
            }

            if (!string.IsNullOrEmpty(user.FullName))
            {
                identity.AddClaim(new Claim("FullName", user.FullName));
            }

            return identity;
        }
    }
}

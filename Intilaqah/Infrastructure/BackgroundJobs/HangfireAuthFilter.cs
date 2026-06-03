using Hangfire.Dashboard;

namespace Intilaqah.Infrastructure.BackgroundJobs
{
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            // Only SuperAdmin can access Hangfire dashboard
            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("SuperAdmin");
        }
    }
}

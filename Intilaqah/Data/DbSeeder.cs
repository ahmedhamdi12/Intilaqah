using Intilaqah.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Data
{
    public static class DbSeeder
    {
        public const string RoleSuperAdmin = "SuperAdmin";
        public const string RoleCompanyAdmin = "CompanyAdmin";
        public const string RoleEmployee = "Employee";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created/migrated
            await context.Database.MigrateAsync();

            // Seed Roles
            await EnsureRoleAsync(roleManager, RoleSuperAdmin, "المدير الأعلى للمنصة");
            await EnsureRoleAsync(roleManager, RoleCompanyAdmin, "مدير المنشأة");
            await EnsureRoleAsync(roleManager, RoleEmployee, "موظف");

            // Seed Permissions (if none)
            if (!await context.Set<Permission>().AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    new Permission { Name = "companies.manage", Group = "SuperAdmin", Description = "إدارة الشركات" },
                    new Permission { Name = "plans.manage", Group = "SuperAdmin", Description = "إدارة الباقات" },
                    new Permission { Name = "employees.manage", Group = "CompanyAdmin", Description = "إدارة الموظفين" },
                    new Permission { Name = "documents.manage", Group = "CompanyAdmin", Description = "إدارة الوثائق" }
                };

                context.Set<Permission>().AddRange(permissions);
                await context.SaveChangesAsync();

                // Assign all SuperAdmin permissions to this role
                var superAdminRole = await roleManager.FindByNameAsync(RoleSuperAdmin);
                if (superAdminRole != null)
                {
                    foreach (var permission in permissions.Where(p => p.Group == "SuperAdmin"))
                    {
                        context.Set<RolePermission>().Add(new RolePermission
                        {
                            RoleId = superAdminRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                    await context.SaveChangesAsync();
                }
            }

            // Seed default SuperAdmin user
            const string superAdminEmail = "admin@intilaqah.sa";
            if (await userManager.FindByEmailAsync(superAdminEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = "مدير النظام",
                    EmailConfirmed = true,
                    IsActive = true,
                    TenantId = null
                };

                var result = await userManager.CreateAsync(user, "Admin@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleSuperAdmin);
                }
            }

            // Seed Plans
            if (!await context.Set<Plan>().AnyAsync())
            {
                var plans = new List<Plan>
                {
                    new Plan
                    {
                        Name = "Basic",
                        MaxUsers = 3,
                        MaxEmployees = 25,
                        IsActive = true,
                        TenantId = Guid.Empty,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        FeaturesJson = "[]"
                    },
                    new Plan
                    {
                        Name = "Professional",
                        MaxUsers = 5,
                        MaxEmployees = 100,
                        IsActive = true,
                        TenantId = Guid.Empty,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        FeaturesJson = "[]"
                    },
                    new Plan
                    {
                        Name = "Enterprise",
                        MaxUsers = 15,
                        MaxEmployees = 500,
                        IsActive = true,
                        TenantId = Guid.Empty,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        FeaturesJson = "[]"
                    }
                };

                context.Set<Plan>().AddRange(plans);
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureRoleAsync(RoleManager<AppRole> roleManager, string roleName, string description)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = roleName,
                    Description = description,
                    TenantId = null
                });
            }
        }
    }
}

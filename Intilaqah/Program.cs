using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Intilaqah.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantResolver, TenantResolver>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with custom Role
builder.Services.AddIdentity<ApplicationUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomClaimsFactory>();

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath         = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.AccessDeniedPath  = "/Account/Forbidden";
    options.ExpireTimeSpan    = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly   = true;
});

//unitofwork & repositories
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IShiftAssignmentRepository, ShiftAssignmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Register Custom Claims Factory (Removed, using AddClaimsPrincipalFactory instead)

// Register Supabase Client
builder.Services.AddScoped<Supabase.Client>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var url = config["Supabase:Url"];
    var key = config["Supabase:SecretKey"];
    var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
    return new Supabase.Client(url, key, options);
});
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

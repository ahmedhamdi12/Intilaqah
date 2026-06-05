using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Intilaqah.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Intilaqah.Infrastructure.Security;
using Hangfire;
using Hangfire.SqlServer;
using Intilaqah.Infrastructure.Audit;
using Intilaqah.Infrastructure.BackgroundJobs;
using Intilaqah.Infrastructure.Notifications;
using Intilaqah.Infrastructure.Integrations;
using Intilaqah.Infrastructure.Integrations.Interfaces;
using Intilaqah.Infrastructure.Integrations.Mudad;
using Intilaqah.Infrastructure.Integrations.Qiwa;

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
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IEmployeeBankAccountRepository, EmployeeBankAccountRepository>();
builder.Services.AddScoped<IViolationRuleRepository, ViolationRuleRepository>();
builder.Services.AddScoped<IViolationRecordRepository, ViolationRecordRepository>();
builder.Services.AddScoped<ISalaryAdvanceRepository, SalaryAdvanceRepository>();
builder.Services.AddScoped<ISalaryAdvanceTransactionRepository, SalaryAdvanceTransactionRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Payroll Rules & Engine
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollRule, Intilaqah.Services.Payroll.Rules.LateDeductionRule>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollRule, Intilaqah.Services.Payroll.Rules.AbsenceDeductionRule>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollRule, Intilaqah.Services.Payroll.Rules.OvertimeRule>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollRule, Intilaqah.Services.Payroll.Rules.ViolationDeductionRule>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollRule, Intilaqah.Services.Payroll.Rules.AdvanceDeductionRule>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollEngine, Intilaqah.Services.Payroll.PayrollEngine>();

// Payroll Services
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollService, Intilaqah.Services.Payroll.PayrollService>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IWpsExportService, Intilaqah.Services.Payroll.WpsExportService>();
builder.Services.AddScoped<Intilaqah.Services.Payroll.IPayrollReportExportService, Intilaqah.Services.Payroll.PayrollReportExportService>();

// Register Supabase Client
builder.Services.AddScoped<Supabase.Client>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var url = config["Supabase:Url"];
    var key = config["Supabase:SecretKey"];
    var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
    return new Supabase.Client(url, key, options);
});

// ── Infrastructure Services ───────────────────────────────────
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INitaqatService, NitaqatService>();
builder.Services.AddScoped<IDocumentAlertService, DocumentAlertService>();
builder.Services.AddScoped<DocumentExpiryJob>();

// ── Integration Services ──────────────────────────────────────
// STUBS — replace with real implementations when API keys available
builder.Services.AddScoped<IQiwaService,  QiwaServiceStub>();
builder.Services.AddScoped<IMudadService, MudadServiceStub>();
builder.Services.AddScoped<IIntegrationSettingsService, IntegrationSettingsService>();

// Background jobs
builder.Services.AddScoped<IntegrationSyncJob>();

// ── Hangfire ──────────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
            QueuePollInterval            = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks           = true,
        }));

builder.Services.AddHangfireServer();

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

// ── Hangfire Dashboard (SuperAdmin only) ──────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Intilaqah — Background Jobs",
    Authorization  = new[] { new HangfireAuthFilter() }
});

// ── Register Recurring Jobs ───────────────────────────────────
RecurringJob.AddOrUpdate<DocumentExpiryJob>(
    "document-expiry-check",
    job => job.RunAsync(),
    "0 8 * * *");  // Daily at 8:00 AM

RecurringJob.AddOrUpdate<IntegrationSyncJob>(
    "integration-sync-retry",
    job => job.RunAsync(),
    "*/30 * * * *");  // Every 30 minutes

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHangfireDashboard();

app.Run();

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using HBDrop.WebApp.Data;
using HBDrop.WebApp.Models;
using HBDrop.WebApp.Services;
using HBDrop.WebApp.Jobs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add authentication state provider for Blazor Server
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

// Configure PostgreSQL database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure Redis connection
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";

// Configure distributed cache for sessions (prevents logout on redeploy)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "HBDrop_";
});

// Configure Data Protection to persist keys in Redis (prevents logout on redeploy)
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddDataProtection()
    .SetApplicationName("HBDrop")
    .PersistKeysToStackExchangeRedis(redis, "HBDrop-DataProtection-Keys");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production with email service
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure authentication cookie to persist across deployments
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "HBDrop.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure Hangfire for background jobs
builder.Services.AddHangfire(config =>
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c =>
            c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// Register custom services
builder.Services.AddHttpContextAccessor(); // Required for getting current user in services
builder.Services.AddScoped<SessionEncryptionService>();
builder.Services.AddHttpClient<IWhatsAppService, BaileysWhatsAppService>();
builder.Services.AddHttpClient<IGifSearchService, GiphySearchService>();
builder.Services.AddHttpClient<AIMessageService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<BirthdayCheckerJob>();
builder.Services.AddScoped<CalendarImportService>();
builder.Services.AddSingleton<RegionalEventsService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres")
    .AddRedis(redisConnection, name: "redis");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Disable HSTS for Docker environment
    // app.UseHsts();
}

// Disable HTTPS redirect for Docker environment
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

// Configure Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule recurring birthday check job (runs every hour)
RecurringJob.AddOrUpdate<HBDrop.WebApp.Jobs.BirthdayCheckerJob>(
    "birthday-checker",
    job => job.CheckAndSendBirthdayWishesAsync(),
    Cron.Hourly); // Run every hour to check timezones

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Run database migrations and initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Run migrations automatically
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        
        // Initialize roles
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();

// Hangfire authorization filter - allow only Admin role
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true 
            && httpContext.User.IsInRole("Admin");
    }
}


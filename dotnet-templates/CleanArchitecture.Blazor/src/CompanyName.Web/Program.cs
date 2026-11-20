using System.Globalization;
using System.Security.Claims;
using CompanyName.Application;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Menus.Queries.GetAllMenus;
using CompanyName.Infrastructure;
using CompanyName.Infrastructure.Data;
using CompanyName.Infrastructure.Persistence;
using CompanyName.Shared.Configurations;
using CompanyName.Web.Components;
using CompanyName.Web.Endpoints;
using CompanyName.Web.Logging;
using CompanyName.Web.Middleware;
using CompanyName.Web.Services;
using Mediator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Radzen;
using Serilog;
using Serilog.Events;

InformixAssemblyBootstrap.RegisterAssemblyResolver();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);

    var fileLogging = context.Configuration.GetSection("Serilog:FileLogging");
    if (fileLogging.GetValue("Enabled", true))
    {
        var logsRoot = ResolveFileLoggingRootPath(
            fileLogging.GetValue<string>("RootPath"),
            context.HostingEnvironment.ContentRootPath);

        static string DailyLogPath(DateTimeOffset timestamp) =>
            Path.Combine(
                timestamp.ToString("MMM", CultureInfo.InvariantCulture),
                $"{timestamp:ddyyyy}.log");

        loggerConfiguration
            .WriteTo.Async(a => a.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level < LogEventLevel.Error)
                .WriteTo.Map(
                    e => DailyLogPath(e.Timestamp),
                    (path, wt) => wt.File(
                        Path.Combine(logsRoot, "info", path),
                        rollingInterval: RollingInterval.Infinite,
                        shared: true))))
            .WriteTo.Async(a => a.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                .WriteTo.Map(
                    e => DailyLogPath(e.Timestamp),
                    (path, wt) => wt.File(
                        Path.Combine(logsRoot, "error", path),
                        rollingInterval: RollingInterval.Infinite,
                        shared: true))));
    }

    var seq = context.Configuration.GetSection("Serilog:Seq");
    if (seq.GetValue("Enabled", false))
    {
        var serverUrl = seq.GetValue<string>("ServerUrl");
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            Console.WriteLine("Serilog Seq is enabled but ServerUrl is missing; Seq sink was not configured.");
        }
        else
        {
            var apiKey = seq.GetValue<string>("ApiKey");
            var batchPostingLimit = seq.GetValue("BatchPostingLimit", 1000);
            var periodSeconds = seq.GetValue("PeriodSeconds", 2);

            loggerConfiguration.WriteTo.Async(a => a.Seq(
                serverUrl,
                apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey,
                batchPostingLimit: batchPostingLimit,
                period: TimeSpan.FromSeconds(periodSeconds)));
        }
    }
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    });

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CompanyName.Web.Services.XmlLocalizationService>();
builder.Services.AddScoped<CompanyName.Web.Services.AppThemeService>();
builder.Services.AddScoped<CompanyName.Web.Services.AlertService>();
builder.Services.AddScoped<LoadingIndicatorService>();
builder.Services.AddScoped<UserStateService>();
builder.Services.AddScoped<BrowserNavigationService>();
builder.Services.AddScoped<CompanyName.Application.Services.TestTransactionService>();
builder.Services.AddScoped<CompanyName.Application.Services.GlUserService>();

builder.Services.AddRadzenComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CompanyName.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseRequestContextLogging();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
    {
        if (ex is not null)
        {
            return LogEventLevel.Error;
        }

        if (StaticContentRequestLogging.IsStaticContent(httpContext.Request))
        {
            return LogEventLevel.Verbose;
        }

        return LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
    };
});
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMenuAuthorization();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAuthEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("Ensuring database exists, applying migrations, and seeding if empty...");
        await DatabaseInitializer.MigrateTemplateDatabaseAsync(dbContext);
    }

    //ensure all parameter cache and load properly
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    await mediator.Send(new GetAllMenusQuery());
}

var encryption = app.Services.GetRequiredService<IEncryptionService>();
var encryptedText = encryption.Encrypt("Server=localhost,1433;Database=TemplateDatabase;User Id=sa;Password=P@ssw0rd;Encrypt=True;TrustServerCertificate=True;");
Console.WriteLine($"Encrypted Text: {encryptedText}");

try
{
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}

static string ResolveFileLoggingRootPath(string? rootPath, string contentRootPath)
{
    rootPath = string.IsNullOrWhiteSpace(rootPath) ? "logs" : rootPath.Trim();

    if (!Path.IsPathRooted(rootPath))
    {
        return Path.GetFullPath(Path.Combine(contentRootPath, rootPath));
    }

    // On Windows, "/logs" is treated as rooted and becomes C:\logs; treat it as app-relative instead.
    if (OperatingSystem.IsWindows()
        && rootPath.StartsWith('/')
        && (rootPath.Length == 1 || rootPath[1] != ':'))
    {
        return Path.GetFullPath(Path.Combine(contentRootPath, rootPath.TrimStart('/', '\\')));
    }

    return Path.GetFullPath(rootPath);
}

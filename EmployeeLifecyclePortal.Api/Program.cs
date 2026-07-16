using EmployeeLifecyclePortal.Api.Middleware;
using EmployeeLifecyclePortal.Api.Middleware.Logging;
using EmployeeLifecyclePortal.Api.Services;
using EmployeeLifecyclePortal.Application;
using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Logging;
using EmployeeLifecyclePortal.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json;

// ── Bootstrap logger ─────────────────────────────────────────────────────────
// A minimal console logger is created before the host builds so that any fatal
// startup error (missing config, DB unreachable, etc.) is still captured.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting EmployeeLifecyclePortal API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    // Replace the host's built-in logging with Serilog; configuration is read
    // from the "Serilog" section in appsettings.json / appsettings.{env}.json.
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // ── Core services ─────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpContextAccessor();

    // ── Custom services ───────────────────────────────────────────────────────
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

    // ── Authentication ────────────────────────────────────────────────────────
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    // ── Authorization policies ────────────────────────────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            Permissions.Admin,
            policy => policy.RequireRole("Admin"));

        options.AddPolicy(
            Permissions.Manager,
            policy => policy.RequireRole("Manager", "Admin"));

        options.AddPolicy(
            Permissions.HR,
            policy => policy.RequireRole("HR", "Admin"));

        options.AddPolicy(
            Permissions.Employee,
            policy => policy.RequireAuthenticatedUser());
    });

    // ── Application & Infrastructure layers ───────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Health checks ─────────────────────────────────────────────────────────
    builder.Services
        .AddHealthChecks()
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sql-server",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "sql" });

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    // Order matters:
    //  1. CorrelationId  — assigns ID and pushes it into Serilog LogContext
    //  2. RequestLogging — logs arrival & completion (reads CorrelationId from LogContext)
    //  3. ExceptionHandler — catches all unhandled exceptions and logs them
    //  4. Swagger (dev only)
    //  5. HTTPS redirection
    //  6. Auth
    //  7. Controllers

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<ApiExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── Health check endpoints ────────────────────────────────────────────────
    // /health         — simple liveness probe (returns 200 / 503)
    // /health/detail  — detailed readiness probe with component statuses (JSON)
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/detail", new HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    Log.Information("EmployeeLifecyclePortal API started successfully.");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "EmployeeLifecyclePortal API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

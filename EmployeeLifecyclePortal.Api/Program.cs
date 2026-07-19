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

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen();

    builder.Services.AddHttpContextAccessor();

    // -------------------------
    // CORS
    // -------------------------
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer =
                        builder.Configuration["Jwt:Issuer"],

                    ValidAudience =
                        builder.Configuration["Jwt:Audience"],

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                builder.Configuration["Jwt:Key"]!))
                };
        });

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

    builder.Services.AddApplication();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services
        .AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sql-server",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "sql" });

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseMiddleware<ApiExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();

        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // -------------------------
    // CORS MUST BE HERE
    // -------------------------
    app.UseCors("Frontend");

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

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
        ResponseWriter =
            HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
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
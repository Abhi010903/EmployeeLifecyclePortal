using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeLifecyclePortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "DefaultConnection"));
        });

        services.AddScoped<IApplicationDbContext>(
            provider =>
                provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
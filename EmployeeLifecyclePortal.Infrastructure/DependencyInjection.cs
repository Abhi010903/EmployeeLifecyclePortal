using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using EmployeeLifecyclePortal.Infrastructure.Repositories;
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

        services.AddScoped<IUnitOfWork,
            UnitOfWork>();

        services.AddScoped<IEmployeeRepository,
            EmployeeRepository>();

        services.AddScoped<IDepartmentRepository,
            DepartmentRepository>();

        services.AddScoped<IRoleRepository,
            RoleRepository>();

        return services;
    }
}
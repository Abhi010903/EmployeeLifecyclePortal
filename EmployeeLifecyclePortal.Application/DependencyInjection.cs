using EmployeeLifecyclePortal.Application.Behaviors;
using EmployeeLifecyclePortal.Application.Commands.Employees;
using EmployeeLifecyclePortal.Application.Services.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeLifecyclePortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(CreateEmployeeCommand).Assembly);
        });

        services.AddValidatorsFromAssembly(
            typeof(CreateEmployeeCommand).Assembly);

        // ValidationBehavior runs first so invalid requests never reach handlers.
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // LoggingBehavior runs after validation so only valid requests are timed and logged.
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        return services;
    }
}

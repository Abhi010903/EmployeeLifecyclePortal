using EmployeeLifecyclePortal.Application.Behaviors;
using EmployeeLifecyclePortal.Application.Commands.Employees;
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

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
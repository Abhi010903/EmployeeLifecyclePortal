using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class AssignManagerCommandHandler
    : IRequestHandler<AssignManagerCommand, Unit>
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<AssignManagerCommandHandler> _logger;

    public AssignManagerCommandHandler(
        IEmployeeService employeeService,
        ILogger<AssignManagerCommandHandler> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        AssignManagerCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assigning manager — Employee: {EmployeeId} | Manager: {ManagerId}",
            request.EmployeeId,
            request.ManagerId);

        await _employeeService.AssignManagerAsync(
            request.EmployeeId,
            request.ManagerId,
            cancellationToken);

        _logger.LogInformation(
            "Manager assigned successfully — Employee: {EmployeeId}",
            request.EmployeeId);

        return Unit.Value;
    }
}

using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record DeactivateEmployeeCommand(
    Guid Id)
    : IRequest;
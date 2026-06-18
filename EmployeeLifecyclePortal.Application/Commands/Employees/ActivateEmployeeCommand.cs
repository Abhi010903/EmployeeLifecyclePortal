using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record ActivateEmployeeCommand(
    Guid Id)
    : IRequest;
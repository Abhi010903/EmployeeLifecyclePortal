using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record DeleteEmployeeCommand(
    Guid Id)
    : IRequest;
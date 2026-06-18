using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record TerminateEmployeeCommand(
    Guid Id)
    : IRequest;
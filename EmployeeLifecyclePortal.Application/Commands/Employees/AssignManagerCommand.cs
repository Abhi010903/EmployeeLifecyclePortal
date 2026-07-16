using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record AssignManagerCommand(
    Guid EmployeeId,
    Guid ManagerId)
    : IRequest<Unit>;

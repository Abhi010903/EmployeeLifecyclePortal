using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Roles;

public sealed record DeleteRoleCommand(
    Guid Id)
    : IRequest;
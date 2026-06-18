using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed record DeleteDepartmentCommand(
    Guid Id)
    : IRequest;
using EmployeeLifecyclePortal.Application.DTOs.Departments;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Departments;

public sealed record GetDepartmentByIdQuery(
    Guid Id)
    : IRequest<DepartmentDto>;
using EmployeeLifecyclePortal.Application.DTOs.ReadModels;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed record GetEmployeeRolesQuery(
    Guid EmployeeId)
    : IRequest<EmployeeRolesDto>;
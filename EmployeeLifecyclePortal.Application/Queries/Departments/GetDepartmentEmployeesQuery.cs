using EmployeeLifecyclePortal.Application.DTOs.ReadModels;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Departments;

public sealed record GetDepartmentEmployeesQuery(
    Guid DepartmentId)
    : IRequest<DepartmentEmployeesDto>;
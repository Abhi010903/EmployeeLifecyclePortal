using EmployeeLifecyclePortal.Application.DTOs.Departments;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Departments;

public sealed record GetAllDepartmentsQuery
    : IRequest<List<DepartmentDto>>;
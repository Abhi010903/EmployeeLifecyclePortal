using EmployeeLifecyclePortal.Application.DTOs.Employees;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed record GetAllEmployeesQuery
    : IRequest<List<EmployeeDto>>;
using EmployeeLifecyclePortal.Application.DTOs.Employees;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed record GetEmployeeByIdQuery(
    Guid Id)
    : IRequest<EmployeeDto>;
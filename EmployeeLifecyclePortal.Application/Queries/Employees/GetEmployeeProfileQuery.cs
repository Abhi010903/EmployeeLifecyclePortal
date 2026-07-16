using EmployeeLifecyclePortal.Application.DTOs;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed record GetEmployeeProfileQuery(Guid EmployeeId)
    : IRequest<EmployeeProfileDto>;

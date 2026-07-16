using EmployeeLifecyclePortal.Application.DTOs;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed record GetEmployeeTimelineQuery(Guid EmployeeId)
    : IRequest<List<EmployeeTimelineDto>>;

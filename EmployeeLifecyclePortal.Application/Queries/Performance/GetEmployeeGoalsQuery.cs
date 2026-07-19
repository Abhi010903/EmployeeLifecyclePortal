using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Performance;

public sealed record GetEmployeeGoalsQuery(Guid EmployeeId)
    : IRequest<List<PerformanceGoalDto>>;

using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record UpdateGoalProgressCommand(
    Guid GoalId,
    int ProgressPercentage)
    : IRequest<PerformanceGoalDto>;

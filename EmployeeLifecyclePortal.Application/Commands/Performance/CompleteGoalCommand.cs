using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record CompleteGoalCommand(Guid GoalId)
    : IRequest<PerformanceGoalDto>;

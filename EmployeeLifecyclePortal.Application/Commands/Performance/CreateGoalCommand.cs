using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record CreateGoalCommand(
    Guid EmployeeId,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate)
    : IRequest<PerformanceGoalDto>;

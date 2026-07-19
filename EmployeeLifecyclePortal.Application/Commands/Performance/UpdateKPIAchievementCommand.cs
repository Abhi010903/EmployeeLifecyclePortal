using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record UpdateKPIAchievementCommand(
    Guid KPIId,
    decimal Achieved)
    : IRequest<KPIDto>;

using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record CreateKPICommand(
    Guid EmployeeId,
    string Name,
    decimal Target,
    int Year)
    : IRequest<KPIDto>;

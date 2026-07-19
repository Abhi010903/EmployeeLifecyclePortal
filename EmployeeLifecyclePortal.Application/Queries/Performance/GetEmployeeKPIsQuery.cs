using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Performance;

public sealed record GetEmployeeKPIsQuery(Guid EmployeeId)
    : IRequest<List<KPIDto>>;

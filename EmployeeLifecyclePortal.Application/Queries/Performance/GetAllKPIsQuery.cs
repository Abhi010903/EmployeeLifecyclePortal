using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Performance;

public sealed record GetAllKPIsQuery()
    : IRequest<List<KPIDto>>;

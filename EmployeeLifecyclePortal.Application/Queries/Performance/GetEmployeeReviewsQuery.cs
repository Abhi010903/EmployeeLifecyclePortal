using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Performance;

public sealed record GetEmployeeReviewsQuery(Guid EmployeeId)
    : IRequest<List<PerformanceReviewDto>>;

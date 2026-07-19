using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record SubmitReviewCommand(
    Guid EmployeeId,
    int Year,
    int Quarter,
    int Rating,
    string? Comments = null)
    : IRequest<PerformanceReviewDto>;

using EmployeeLifecyclePortal.Application.DTOs.Performance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Performance;

public sealed record ApproveReviewCommand(
    Guid ReviewId,
    Guid ReviewedByUserId)
    : IRequest<PerformanceReviewDto>;

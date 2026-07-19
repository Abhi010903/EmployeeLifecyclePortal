using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Performance.Handlers;

public sealed class ApproveReviewCommandHandler
    : IRequestHandler<ApproveReviewCommand, PerformanceReviewDto>
{
    private readonly IApplicationDbContext _context;

    public ApproveReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PerformanceReviewDto> Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException($"Review with ID {request.ReviewId} not found");

        review.Approve(request.ReviewedByUserId);
        await _context.SaveChangesAsync(cancellationToken);

        return new PerformanceReviewDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            EmployeeName = review.Employee != null 
                ? $"{review.Employee.FirstName} {review.Employee.LastName}"
                : "Unknown",
            ReviewedByUserId = review.ReviewedByUserId,
            Year = review.Year,
            Quarter = review.Quarter,
            Rating = review.Rating,
            Comments = review.Comments,
            Status = review.Status,
            CreatedAtUtc = review.CreatedAtUtc,
            CreatedBy = review.CreatedBy,
            LastModifiedAtUtc = review.LastModifiedAtUtc,
            LastModifiedBy = review.LastModifiedBy
        };
    }
}

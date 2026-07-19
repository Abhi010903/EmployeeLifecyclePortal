using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Performance.Handlers;

public sealed class SubmitReviewCommandHandler
    : IRequestHandler<SubmitReviewCommand, PerformanceReviewDto>
{
    private readonly IApplicationDbContext _context;

    public SubmitReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PerformanceReviewDto> Handle(
        SubmitReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = new PerformanceReview(
            request.EmployeeId,
            request.Year,
            request.Quarter,
            request.Rating);

        if (!string.IsNullOrEmpty(request.Comments))
        {
            review.SetComments(request.Comments);
        }

        review.Submit();
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        return new PerformanceReviewDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            EmployeeName = employee != null 
                ? $"{employee.FirstName} {employee.LastName}"
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

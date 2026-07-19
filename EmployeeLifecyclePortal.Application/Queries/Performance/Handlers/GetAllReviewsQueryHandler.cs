using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Performance.Handlers;

public sealed class GetAllReviewsQueryHandler
    : IRequestHandler<GetAllReviewsQuery, List<PerformanceReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PerformanceReviewDto>> Handle(
        GetAllReviewsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Select(review => new PerformanceReviewDto
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
            })
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .ToListAsync(cancellationToken);
    }
}

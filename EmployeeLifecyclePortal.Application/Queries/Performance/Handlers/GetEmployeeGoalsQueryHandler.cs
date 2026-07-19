using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Performance.Handlers;

public sealed class GetEmployeeGoalsQueryHandler
    : IRequestHandler<GetEmployeeGoalsQuery, List<PerformanceGoalDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeGoalsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PerformanceGoalDto>> Handle(
        GetEmployeeGoalsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PerformanceGoals
            .Include(g => g.Employee)
            .Where(g => g.EmployeeId == request.EmployeeId)
            .Select(goal => new PerformanceGoalDto
            {
                Id = goal.Id,
                EmployeeId = goal.EmployeeId,
                EmployeeName = goal.Employee != null 
                    ? $"{goal.Employee.FirstName} {goal.Employee.LastName}"
                    : "Unknown",
                Title = goal.Title,
                Description = goal.Description,
                StartDateUtc = goal.StartDateUtc,
                EndDateUtc = goal.EndDateUtc,
                Status = goal.Status,
                ProgressPercentage = goal.ProgressPercentage,
                CreatedAtUtc = goal.CreatedAtUtc,
                CreatedBy = goal.CreatedBy,
                LastModifiedAtUtc = goal.LastModifiedAtUtc,
                LastModifiedBy = goal.LastModifiedBy
            })
            .OrderByDescending(g => g.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}

using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Performance.Handlers;

public sealed class UpdateGoalProgressCommandHandler
    : IRequestHandler<UpdateGoalProgressCommand, PerformanceGoalDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateGoalProgressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PerformanceGoalDto> Handle(
        UpdateGoalProgressCommand request,
        CancellationToken cancellationToken)
    {
        var goal = await _context.PerformanceGoals
            .Include(g => g.Employee)
            .FirstOrDefaultAsync(g => g.Id == request.GoalId, cancellationToken)
            ?? throw new InvalidOperationException($"Goal with ID {request.GoalId} not found");

        goal.UpdateProgress(request.ProgressPercentage);
        await _context.SaveChangesAsync(cancellationToken);

        return new PerformanceGoalDto
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
        };
    }
}

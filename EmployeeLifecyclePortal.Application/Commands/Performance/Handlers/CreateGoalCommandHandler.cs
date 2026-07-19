using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Performance.Handlers;

public sealed class CreateGoalCommandHandler
    : IRequestHandler<CreateGoalCommand, PerformanceGoalDto>
{
    private readonly IApplicationDbContext _context;

    public CreateGoalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PerformanceGoalDto> Handle(
        CreateGoalCommand request,
        CancellationToken cancellationToken)
    {
        var goal = new PerformanceGoal(
            request.EmployeeId,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate);

        _context.PerformanceGoals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        return new PerformanceGoalDto
        {
            Id = goal.Id,
            EmployeeId = goal.EmployeeId,
            EmployeeName = employee != null 
                ? $"{employee.FirstName} {employee.LastName}"
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

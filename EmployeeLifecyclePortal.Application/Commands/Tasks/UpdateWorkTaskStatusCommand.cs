using EmployeeLifecyclePortal.Application.DTOs.Tasks;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Tasks;

public sealed record UpdateWorkTaskStatusCommand(
    Guid Id,
    int CompletionPercentage,
    string Status,
    string? Comments = null)
    : IRequest<WorkTaskDto>;

public sealed class UpdateWorkTaskStatusCommandHandler : IRequestHandler<UpdateWorkTaskStatusCommand, WorkTaskDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkTaskStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkTaskDto> Handle(UpdateWorkTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.WorkTasks
            .Include(t => t.Employee)
            .Include(t => t.Department)
            .Include(t => t.Manager)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
            throw new InvalidOperationException("Task not found.");

        task.UpdateProgress(request.CompletionPercentage, request.Status, request.Comments);
        await _context.SaveChangesAsync(cancellationToken);

        return new WorkTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            EmployeeId = task.EmployeeId,
            EmployeeName = task.Employee?.FullName ?? "Unknown",
            DepartmentId = task.DepartmentId,
            DepartmentName = task.Department?.Name,
            ManagerId = task.ManagerId,
            ManagerName = task.Manager?.FullName,
            Priority = task.Priority,
            StartDateUtc = task.StartDateUtc,
            DeadlineUtc = task.DeadlineUtc,
            Status = task.Status,
            CompletionPercentage = task.CompletionPercentage,
            Comments = task.Comments,
            CompletedAtUtc = task.CompletedAtUtc,
            CreatedAtUtc = task.CreatedAtUtc
        };
    }
}

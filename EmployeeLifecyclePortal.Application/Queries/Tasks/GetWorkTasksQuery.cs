using EmployeeLifecyclePortal.Application.DTOs.Tasks;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Tasks;

public sealed record GetWorkTasksQuery(
    Guid? EmployeeId = null,
    Guid? DepartmentId = null,
    string? Status = null)
    : IRequest<List<WorkTaskDto>>;

public sealed class GetWorkTasksQueryHandler : IRequestHandler<GetWorkTasksQuery, List<WorkTaskDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkTaskDto>> Handle(GetWorkTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkTasks
            .Include(t => t.Employee)
            .Include(t => t.Department)
            .Include(t => t.Manager)
            .AsNoTracking();

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            query = query.Where(t => t.EmployeeId == request.EmployeeId.Value);
        }

        if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
        {
            query = query.Where(t => t.DepartmentId == request.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "All")
        {
            query = query.Where(t => t.Status == request.Status);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return tasks.Select(task => new WorkTaskDto
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
        }).ToList();
    }
}

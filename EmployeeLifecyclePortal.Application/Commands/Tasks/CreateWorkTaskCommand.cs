using EmployeeLifecyclePortal.Application.DTOs.Tasks;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Tasks;

public sealed record CreateWorkTaskCommand(
    string Title,
    string Description,
    Guid EmployeeId,
    string Priority,
    DateTime StartDateUtc,
    DateTime DeadlineUtc)
    : IRequest<WorkTaskDto>;

public sealed class CreateWorkTaskCommandHandler : IRequestHandler<CreateWorkTaskCommand, WorkTaskDto>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkTaskDto> Handle(CreateWorkTaskCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            throw new InvalidOperationException("Target employee not found.");

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == employee.DepartmentId, cancellationToken);

        var task = new WorkTask(
            request.Title,
            request.Description,
            employee.Id,
            employee.DepartmentId,
            employee.ManagerId,
            string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
            request.StartDateUtc,
            request.DeadlineUtc);

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new WorkTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            DepartmentId = department?.Id,
            DepartmentName = department?.Name,
            ManagerId = employee.ManagerId,
            ManagerName = employee.Manager?.FullName,
            Priority = task.Priority,
            StartDateUtc = task.StartDateUtc,
            DeadlineUtc = task.DeadlineUtc,
            Status = task.Status,
            CompletionPercentage = task.CompletionPercentage,
            Comments = task.Comments,
            CreatedAtUtc = task.CreatedAtUtc
        };
    }
}

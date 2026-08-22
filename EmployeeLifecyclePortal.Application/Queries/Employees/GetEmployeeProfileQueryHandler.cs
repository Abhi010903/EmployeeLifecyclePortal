using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed class GetEmployeeProfileQueryHandler
    : IRequestHandler<GetEmployeeProfileQuery, EmployeeProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetEmployeeProfileQueryHandler> _logger;

    public GetEmployeeProfileQueryHandler(
        IApplicationDbContext context,
        ILogger<GetEmployeeProfileQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmployeeProfileDto> Handle(
        GetEmployeeProfileQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Manager)
            .Include(e => e.TeamLead)
            .FirstOrDefaultAsync(
                x => x.Id == request.EmployeeId,
                cancellationToken);

        if (employee is null)
            throw new InvalidOperationException(
                $"Employee with ID {request.EmployeeId} not found.");

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == employee.DepartmentId, cancellationToken);

        var roles = await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employee.Id)
            .Join(_context.Roles,
                er => er.RoleId,
                r => r.Id,
                (er, r) => new EmployeeRoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleDescription = r.Description
                })
            .ToListAsync(cancellationToken);

        var timelineEventsCount = await _context.EmployeeTimelines
            .CountAsync(t => t.EmployeeId == employee.Id, cancellationToken);

        var documentsCount = await _context.EmployeeDocuments
            .CountAsync(d => d.EmployeeId == employee.Id && !d.IsArchived, cancellationToken);

        var subordinatesCount = await _context.Employees
            .CountAsync(e => e.ManagerId == employee.Id || e.TeamLeadId == employee.Id, cancellationToken);

        _logger.LogInformation(
            "Employee profile retrieved — Employee: {EmployeeId} | Name: {FullName} | Roles: {RoleCount}",
            request.EmployeeId,
            employee.FullName,
            roles.Count);

        return new EmployeeProfileDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Status = employee.Status.ToString(),
            DepartmentId = employee.DepartmentId,
            DepartmentName = department?.Name,
            ManagerId = employee.ManagerId,
            ManagerName = employee.Manager?.FullName,
            TeamLeadId = employee.TeamLeadId,
            TeamLeadName = employee.TeamLead?.FullName,
            Roles = roles,
            CreatedAtUtc = employee.CreatedAtUtc,
            CreatedBy = employee.CreatedBy,
            LastModifiedAtUtc = employee.LastModifiedAtUtc,
            LastModifiedBy = employee.LastModifiedBy,
            TimelineEventsCount = timelineEventsCount,
            DocumentsCount = documentsCount,
            SubordinatesCount = subordinatesCount
        };
    }
}

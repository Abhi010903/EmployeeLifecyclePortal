using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard.Handlers;

/// <summary>
/// Handles the GetRecentActivityQuery to retrieve recent audit log activities.
/// Returns the latest HR actions performed in the system.
/// </summary>
public sealed class GetRecentActivityQueryHandler
    : IRequestHandler<GetRecentActivityQuery, List<RecentActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentActivityQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentActivityDto>> Handle(
        GetRecentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = await _context.AuditLogs
            .OrderByDescending(al => al.OperatedAtUtc)
            .Take(request.Limit)
            .Select(al => new RecentActivityDto
            {
                Id = al.Id,
                ActivityType = MapActivityType(al.EntityType, al.Operation),
                Description = MapActivityDescription(al.EntityType, al.Operation),
                PerformedBy = al.OperatedBy,
                OccurredAtUtc = al.OperatedAtUtc,
                Category = MapActivityCategory(al.EntityType)
            })
            .ToListAsync(cancellationToken);

        return activities;
    }

    /// <summary>
    /// Maps the entity type and operation to a user-friendly activity type.
    /// </summary>
    private static string MapActivityType(string entityType, string operation)
    {
        return $"{entityType} {operation}";
    }

    /// <summary>
    /// Maps the entity type and operation to a descriptive message.
    /// </summary>
    private static string MapActivityDescription(string entityType, string operation)
    {
        return operation switch
        {
            "Created" => $"New {entityType.ToLower()} created",
            "Updated" => $"{entityType} information updated",
            "Deleted" => $"{entityType} deleted",
            _ => $"{entityType} {operation.ToLower()}"
        };
    }

    /// <summary>
    /// Maps the entity type to an activity category for icon/color coding.
    /// </summary>
    private static string MapActivityCategory(string entityType)
    {
        return entityType.ToLower() switch
        {
            "employee" => "employee",
            "department" => "department",
            "role" => "role",
            "leaverequest" => "leave",
            "attendance" => "attendance",
            "payslip" => "payroll",
            "certificate" => "training",
            "asset" => "asset",
            _ => "other"
        };
    }
}

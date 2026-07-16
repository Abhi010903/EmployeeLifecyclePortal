using EmployeeLifecyclePortal.Application.DTOs;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.AuditLogs;

/// <summary>
/// Query to retrieve audit logs with optional filtering and pagination.
/// </summary>
public sealed record GetAuditLogsQuery(
    string? EntityType = null,
    string? Operation = null,
    int PageNumber = 1,
    int PageSize = 50)
    : IRequest<GetAuditLogsResponse>;

public sealed record GetAuditLogsResponse(
    List<AuditLogDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

using EmployeeLifecyclePortal.Application.DTOs;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.AuditLogs;

/// <summary>
/// Query to retrieve the complete modification history of a specific entity.
/// Shows all changes to one entity from creation to last modification.
/// </summary>
public sealed record GetEntityHistoryQuery(
    string EntityType,
    string EntityId)
    : IRequest<EntityHistoryDto>;

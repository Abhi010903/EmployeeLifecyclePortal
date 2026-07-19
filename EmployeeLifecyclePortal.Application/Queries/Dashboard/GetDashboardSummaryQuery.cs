using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard;

/// <summary>
/// Query to retrieve dashboard summary statistics only.
/// Contains counts for employees, departments, leave, attendance, and payroll.
/// Lightweight query for individual card updates.
/// </summary>
public sealed record GetDashboardSummaryQuery
    : IRequest<DashboardSummaryDto>;

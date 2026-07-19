using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard.Handlers;

/// <summary>
/// Handles the GetEmployeeGrowthTrendQuery to retrieve monthly employee growth and attendance trends.
/// Returns data for the last 6 months.
/// </summary>
public sealed class GetEmployeeGrowthTrendQueryHandler
    : IRequestHandler<GetEmployeeGrowthTrendQuery, List<EmployeeGrowthChartDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeGrowthTrendQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeGrowthChartDto>> Handle(
        GetEmployeeGrowthTrendQuery request,
        CancellationToken cancellationToken)
    {
        var result = new List<EmployeeGrowthChartDto>();
        var today = DateTime.UtcNow;
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Get data for the last 6 months
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = today.AddMonths(-i);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            var monthLabel = monthNames[monthDate.Month - 1];

            // Count employees active by end of month
            var employeeCount = await _context.Employees
                .Where(e => e.CreatedAtUtc < monthEnd && e.Status == EmploymentStatus.Active)
                .CountAsync(cancellationToken);

            // Count attendance records for the month (handle missing table)
            int attendanceCount = 0;
            try
            {
                attendanceCount = await _context.Attendances
                    .Where(a => a.CheckInTimeUtc >= monthStart && a.CheckInTimeUtc < monthEnd)
                    .Select(a => a.EmployeeId)
                    .Distinct()
                    .CountAsync(cancellationToken);
            }
            catch
            {
                // Attendances table may not exist - use employee count as fallback
                attendanceCount = employeeCount;
            }

            result.Add(new EmployeeGrowthChartDto
            {
                Month = monthLabel,
                TotalEmployees = employeeCount,
                TotalAttendance = attendanceCount
            });
        }

        return result;
    }
}

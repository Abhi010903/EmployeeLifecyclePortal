using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed class GetEmployeeTimelineQueryHandler
    : IRequestHandler<GetEmployeeTimelineQuery, List<EmployeeTimelineDto>>
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<GetEmployeeTimelineQueryHandler> _logger;

    public GetEmployeeTimelineQueryHandler(
        IEmployeeService employeeService,
        ILogger<GetEmployeeTimelineQueryHandler> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    public async Task<List<EmployeeTimelineDto>> Handle(
        GetEmployeeTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var timelines = await _employeeService.GetTimelineEventsAsync(
            request.EmployeeId,
            cancellationToken);

        _logger.LogInformation(
            "Employee timeline retrieved — Employee: {EmployeeId} | Events: {Count}",
            request.EmployeeId,
            timelines.Count);

        return timelines.Select(x => new EmployeeTimelineDto
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EventType = x.EventType,
            Title = x.Title,
            Description = x.Description,
            EventDateUtc = x.EventDateUtc,
            Category = x.Category,
            CreatedAtUtc = x.CreatedAtUtc,
            CreatedBy = x.CreatedBy
        }).ToList();
    }
}

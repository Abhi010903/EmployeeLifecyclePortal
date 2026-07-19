using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Performance.Handlers;

public sealed class CreateKPICommandHandler
    : IRequestHandler<CreateKPICommand, KPIDto>
{
    private readonly IApplicationDbContext _context;

    public CreateKPICommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KPIDto> Handle(
        CreateKPICommand request,
        CancellationToken cancellationToken)
    {
        var kpi = new KPI(
            request.EmployeeId,
            request.Name,
            request.Target,
            request.Year);

        _context.KPIs.Add(kpi);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        return new KPIDto
        {
            Id = kpi.Id,
            EmployeeId = kpi.EmployeeId,
            EmployeeName = employee != null 
                ? $"{employee.FirstName} {employee.LastName}"
                : "Unknown",
            Name = kpi.Name,
            Target = kpi.Target,
            Achieved = kpi.Achieved,
            Year = kpi.Year,
            AchievementPercentage = (decimal)kpi.GetAchievementPercentage(),
            CreatedAtUtc = kpi.CreatedAtUtc,
            CreatedBy = kpi.CreatedBy,
            LastModifiedAtUtc = kpi.LastModifiedAtUtc,
            LastModifiedBy = kpi.LastModifiedBy
        };
    }
}

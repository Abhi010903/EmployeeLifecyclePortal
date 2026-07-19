using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Performance.Handlers;

public sealed class UpdateKPIAchievementCommandHandler
    : IRequestHandler<UpdateKPIAchievementCommand, KPIDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateKPIAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KPIDto> Handle(
        UpdateKPIAchievementCommand request,
        CancellationToken cancellationToken)
    {
        var kpi = await _context.KPIs
            .Include(k => k.Employee)
            .FirstOrDefaultAsync(k => k.Id == request.KPIId, cancellationToken)
            ?? throw new InvalidOperationException($"KPI with ID {request.KPIId} not found");

        kpi.UpdateAchievement(request.Achieved);
        await _context.SaveChangesAsync(cancellationToken);

        return new KPIDto
        {
            Id = kpi.Id,
            EmployeeId = kpi.EmployeeId,
            EmployeeName = kpi.Employee != null 
                ? $"{kpi.Employee.FirstName} {kpi.Employee.LastName}"
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

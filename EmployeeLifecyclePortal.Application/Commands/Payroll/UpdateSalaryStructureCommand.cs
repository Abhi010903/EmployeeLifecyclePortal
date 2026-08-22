using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Payroll;

public sealed record UpdateSalaryStructureCommand(
    Guid EmployeeId,
    decimal BaseSalary) : IRequest<SalaryStructureDto>;

public sealed class UpdateSalaryStructureCommandHandler : IRequestHandler<UpdateSalaryStructureCommand, SalaryStructureDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSalaryStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryStructureDto> Handle(UpdateSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.SalaryStructures
            .Where(s => s.EmployeeId == request.EmployeeId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            existing.End();
        }

        var newStructure = new SalaryStructure(request.EmployeeId, request.BaseSalary, DateTime.UtcNow);
        _context.SalaryStructures.Add(newStructure);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        return new SalaryStructureDto
        {
            Id = newStructure.Id,
            EmployeeId = newStructure.EmployeeId,
            EmployeeName = employee?.FullName,
            BaseSalary = newStructure.BaseSalary,
            Currency = newStructure.Currency,
            EffectiveFromUtc = newStructure.EffectiveFromUtc,
            IsActive = newStructure.IsActive,
            CreatedAtUtc = newStructure.CreatedAtUtc,
            CreatedBy = newStructure.CreatedBy,
            LastModifiedAtUtc = newStructure.LastModifiedAtUtc,
            LastModifiedBy = newStructure.LastModifiedBy
        };
    }
}

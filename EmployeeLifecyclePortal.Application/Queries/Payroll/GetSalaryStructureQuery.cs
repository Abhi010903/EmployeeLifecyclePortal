using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Payroll;

public sealed record GetSalaryStructureQuery(Guid EmployeeId) : IRequest<SalaryStructureDto?>;

public sealed class GetSalaryStructureQueryHandler : IRequestHandler<GetSalaryStructureQuery, SalaryStructureDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryStructureQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryStructureDto?> Handle(GetSalaryStructureQuery request, CancellationToken cancellationToken)
    {
        var structure = await _context.SalaryStructures
            .Include(s => s.Employee)
            .Where(s => s.EmployeeId == request.EmployeeId && s.IsActive)
            .OrderByDescending(s => s.EffectiveFromUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (structure == null)
        {
            // Check if employee exists
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
            if (employee == null) return null;

            // Auto-create initial default salary structure for existing employee
            var defaultStructure = new SalaryStructure(request.EmployeeId, 50000m, DateTime.UtcNow);
            _context.SalaryStructures.Add(defaultStructure);
            await _context.SaveChangesAsync(cancellationToken);

            return new SalaryStructureDto
            {
                Id = defaultStructure.Id,
                EmployeeId = defaultStructure.EmployeeId,
                EmployeeName = employee.FullName,
                BaseSalary = defaultStructure.BaseSalary,
                Currency = defaultStructure.Currency,
                EffectiveFromUtc = defaultStructure.EffectiveFromUtc,
                IsActive = defaultStructure.IsActive,
                CreatedAtUtc = defaultStructure.CreatedAtUtc,
                CreatedBy = defaultStructure.CreatedBy
            };
        }

        return new SalaryStructureDto
        {
            Id = structure.Id,
            EmployeeId = structure.EmployeeId,
            EmployeeName = structure.Employee?.FullName,
            BaseSalary = structure.BaseSalary,
            Currency = structure.Currency,
            EffectiveFromUtc = structure.EffectiveFromUtc,
            EffectiveToUtc = structure.EffectiveToUtc,
            IsActive = structure.IsActive,
            CreatedAtUtc = structure.CreatedAtUtc,
            CreatedBy = structure.CreatedBy,
            LastModifiedAtUtc = structure.LastModifiedAtUtc,
            LastModifiedBy = structure.LastModifiedBy
        };
    }
}

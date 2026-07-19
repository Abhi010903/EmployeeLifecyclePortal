using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (department is null)
        {
            throw new InvalidOperationException($"Department with ID {request.Id} not found.");
        }

        // Update properties
        var departmentType = department.GetType();
        var nameProperty = departmentType.GetProperty("Name");
        var descriptionProperty = departmentType.GetProperty("Description");

        nameProperty?.SetValue(department, request.Name);
        descriptionProperty?.SetValue(department, request.Description);

        _context.Departments.Update(department);
        await _context.SaveChangesAsync(cancellationToken);

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            CreatedAtUtc = department.CreatedAtUtc,
            CreatedBy = department.CreatedBy,
            LastModifiedAtUtc = department.LastModifiedAtUtc,
            LastModifiedBy = department.LastModifiedBy,
        };
    }
}

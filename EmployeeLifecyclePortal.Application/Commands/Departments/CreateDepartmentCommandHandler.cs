using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed class CreateDepartmentCommandHandler
    : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = new Department(
            request.Name,
            request.Description);

        _context.Departments.Add(department);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            CreatedAtUtc = department.CreatedAtUtc,
            CreatedBy = department.CreatedBy,
            LastModifiedAtUtc = department.LastModifiedAtUtc,
            LastModifiedBy = department.LastModifiedBy
        };
    }
}
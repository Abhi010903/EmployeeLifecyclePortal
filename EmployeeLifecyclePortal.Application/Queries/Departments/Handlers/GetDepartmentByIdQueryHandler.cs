using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Departments.Handlers;

public sealed class GetDepartmentByIdQueryHandler
    : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentByIdQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (department is null)
        {
            throw new InvalidOperationException(
                "Department not found.");
        }

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };
    }
}
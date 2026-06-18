using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Departments.Handlers;

public sealed class GetAllDepartmentsQueryHandler
    : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDepartmentsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> Handle(
        GetAllDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Departments
            .Select(x => new DepartmentDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedBy = x.CreatedBy,
                LastModifiedAtUtc = x.LastModifiedAtUtc,
                LastModifiedBy = x.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}
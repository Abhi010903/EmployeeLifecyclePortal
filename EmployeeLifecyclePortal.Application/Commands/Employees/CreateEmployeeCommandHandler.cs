using EmployeeLifecyclePortal.Application.DTOs.Employees;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class CreateEmployeeCommandHandler
    : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEmployeeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = new Employee(
            request.EmployeeCode,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DepartmentId);

        _context.Employees.Add(employee);

        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DepartmentId = employee.DepartmentId
        };
    }
}
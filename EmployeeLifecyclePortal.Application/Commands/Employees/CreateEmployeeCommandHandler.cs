using EmployeeLifecyclePortal.Application.DTOs.Employees;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class CreateEmployeeCommandHandler
    : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
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

        await _employeeRepository.AddAsync(
            employee,
            cancellationToken);

        await _unitOfWork.CommitAsync(
            cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DepartmentId = employee.DepartmentId,
            CreatedAtUtc = employee.CreatedAtUtc,
            CreatedBy = employee.CreatedBy,
            LastModifiedAtUtc = employee.LastModifiedAtUtc,
            LastModifiedBy = employee.LastModifiedBy
        };
    }
}
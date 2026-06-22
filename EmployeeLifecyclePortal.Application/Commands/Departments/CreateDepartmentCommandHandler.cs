using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed class CreateDepartmentCommandHandler
    : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentDto> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = new Department(
            request.Name,
            request.Description);

        await _departmentRepository.AddAsync(
            department,
            cancellationToken);

        await _unitOfWork.CommitAsync(
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
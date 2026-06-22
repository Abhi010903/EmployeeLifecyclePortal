using EmployeeLifecyclePortal.Application.DTOs.Roles;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Roles.Handlers;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = new Role(
            request.Name,
            request.Description);

        await _roleRepository.AddAsync(
            role,
            cancellationToken);

        await _unitOfWork.CommitAsync(
            cancellationToken);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAtUtc = role.CreatedAtUtc,
            CreatedBy = role.CreatedBy,
            LastModifiedAtUtc = role.LastModifiedAtUtc,
            LastModifiedBy = role.LastModifiedBy
        };
    }
}
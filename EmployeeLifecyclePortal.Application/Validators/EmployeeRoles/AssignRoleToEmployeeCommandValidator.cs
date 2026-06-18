using EmployeeLifecyclePortal.Application.Commands.EmployeeRoles;
using FluentValidation;

namespace EmployeeLifecyclePortal.Application.Validators.EmployeeRoles;

public sealed class AssignRoleToEmployeeCommandValidator
    : AbstractValidator<AssignRoleToEmployeeCommand>
{
    public AssignRoleToEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}
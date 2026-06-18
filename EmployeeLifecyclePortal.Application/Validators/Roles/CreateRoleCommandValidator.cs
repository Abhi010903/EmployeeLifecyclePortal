using EmployeeLifecyclePortal.Application.Commands.Roles;
using FluentValidation;

namespace EmployeeLifecyclePortal.Application.Validators.Roles;

public sealed class CreateRoleCommandValidator
    : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
using EmployeeLifecyclePortal.Application.Commands.Departments;
using FluentValidation;

namespace EmployeeLifecyclePortal.Application.Validators.Departments;

public sealed class CreateDepartmentCommandValidator
    : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
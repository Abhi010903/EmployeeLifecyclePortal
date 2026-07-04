using EmployeeLifecyclePortal.Application.Commands.Auth;
using FluentValidation;

namespace EmployeeLifecyclePortal.Application.Validators.Auth;

public sealed class LoginCommandValidator
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
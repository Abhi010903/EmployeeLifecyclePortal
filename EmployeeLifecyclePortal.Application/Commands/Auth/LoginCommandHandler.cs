using EmployeeLifecyclePortal.Application.DTOs.Auth;
using EmployeeLifecyclePortal.Application.Exceptions.Auth;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories.Auth;
using EmployeeLifecyclePortal.Application.Services.Auth;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Auth;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;

    private readonly IJwtTokenService _jwtTokenService;

    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var passwordValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new InvalidCredentialsException();
        }

        var token =
            _jwtTokenService.GenerateToken(
                user.Id,
                user.Email,
                user.Role);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                Name = !string.IsNullOrWhiteSpace(user.Username) ? user.Username : user.Email
            }
        };
    }
}
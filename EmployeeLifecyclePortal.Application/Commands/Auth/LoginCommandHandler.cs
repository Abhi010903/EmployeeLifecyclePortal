using BCrypt.Net;
using EmployeeLifecyclePortal.Application.DTOs.Auth;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories.Auth;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Auth;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
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
            throw new UnauthorizedAccessException(
                "Invalid credentials.");
        }

        var validPassword =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!validPassword)
        {
            throw new UnauthorizedAccessException(
                "Invalid credentials.");
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
            Role = user.Role
        };
    }
}
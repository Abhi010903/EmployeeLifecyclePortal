using BCrypt.Net;
using EmployeeLifecyclePortal.Application.DTOs.Auth;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories.Auth;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Auth;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser =
            await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "User already exists.");
        }

        var passwordHash =
            BCrypt.Net.BCrypt.HashPassword(
                request.Password);

        var user =
            new ApplicationUser(
                request.Username,
                request.Email,
                passwordHash,
                request.Role);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _unitOfWork.CommitAsync(
            cancellationToken);

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
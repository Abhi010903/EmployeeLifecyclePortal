using EmployeeLifecyclePortal.Application.DTOs.Auth;
using EmployeeLifecyclePortal.Application.Exceptions.Auth;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories.Auth;
using EmployeeLifecyclePortal.Application.Services.Auth;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Auth;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IJwtTokenService _jwtTokenService;

    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
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
            throw new UserAlreadyExistsException(
                request.Email);
        }

        var passwordHash =
            _passwordHasher.Hash(
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
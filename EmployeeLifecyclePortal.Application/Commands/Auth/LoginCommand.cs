using EmployeeLifecyclePortal.Application.DTOs.Auth;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Auth;

public sealed record LoginCommand(
    string Email,
    string Password)
    : IRequest<AuthResponseDto>;
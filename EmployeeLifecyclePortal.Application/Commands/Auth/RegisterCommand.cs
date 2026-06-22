using EmployeeLifecyclePortal.Application.DTOs.Auth;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Auth;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string Role)
    : IRequest<AuthResponseDto>;
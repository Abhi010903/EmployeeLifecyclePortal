namespace EmployeeLifecyclePortal.Application.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public UserDto User { get; set; } = new();
}

public sealed class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
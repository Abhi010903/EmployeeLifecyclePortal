namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(
        Guid userId,
        string email,
        string role,
        Guid? employeeId = null,
        string username = "");
}
namespace EmployeeLifecyclePortal.Application.Exceptions.Auth;

public sealed class InvalidCredentialsException
    : Exception
{
    public InvalidCredentialsException()
        : base(
            "Invalid email or password.")
    {
    }
}
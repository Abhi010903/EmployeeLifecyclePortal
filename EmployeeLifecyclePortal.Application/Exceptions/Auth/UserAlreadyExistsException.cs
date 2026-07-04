namespace EmployeeLifecyclePortal.Application.Exceptions.Auth;

public sealed class UserAlreadyExistsException
    : Exception
{
    public UserAlreadyExistsException(
        string email)
        : base(
            $"A user with email '{email}' already exists.")
    {
    }
}
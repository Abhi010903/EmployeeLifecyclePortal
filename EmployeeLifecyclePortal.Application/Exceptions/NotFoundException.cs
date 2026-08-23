namespace EmployeeLifecyclePortal.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message = "The requested resource could not be found.")
        : base(message)
    {
    }
}

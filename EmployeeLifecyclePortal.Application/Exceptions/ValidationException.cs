namespace EmployeeLifecyclePortal.Application.Exceptions;

public sealed class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(
        List<string> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}
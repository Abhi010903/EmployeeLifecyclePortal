using System.Text.Json;

namespace EmployeeLifecyclePortal.Api.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message);
        }
        catch (Exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.ContentType =
            "application/json";

        context.Response.StatusCode =
            statusCode;

        var response = new ExceptionResponse
        {
            StatusCode = statusCode,
            Message = message
        };

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}
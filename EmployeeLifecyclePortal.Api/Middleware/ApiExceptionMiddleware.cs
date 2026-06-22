using System.Net;
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
        catch (Exception ex)
        {
            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType =
                "application/json";

            var result = JsonSerializer.Serialize(
                new
                {
                    Exception = ex.ToString(),
                    Message = ex.Message,
                    InnerException =
                        ex.InnerException?.ToString()
                });

            await context.Response.WriteAsync(
                result);
        }
    }
}
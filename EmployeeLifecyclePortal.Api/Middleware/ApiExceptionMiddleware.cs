using EmployeeLifecyclePortal.Application.Exceptions.Auth;
using EmployeeLifecyclePortal.Application.Exceptions;
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType =
            "application/json";

        context.Response.StatusCode =
            exception switch
            {
                ValidationException =>
                    (int)HttpStatusCode.BadRequest,

                UserAlreadyExistsException =>
                    (int)HttpStatusCode.Conflict,

                InvalidCredentialsException =>
                    (int)HttpStatusCode.Unauthorized,

                _ =>
                    (int)HttpStatusCode.InternalServerError
            };

        var response =
            JsonSerializer.Serialize(
                new
                {
                    StatusCode =
                        context.Response.StatusCode,

                    Message =
                        exception.Message
                });

        return context.Response.WriteAsync(
            response);
    }
}
using System.Text.Json;
using AssignmentSystem.Application.Common.Exceptions;
using FluentValidation;

namespace AssignmentSystem.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, payload) = ex switch
        {
            DuplicateEmailException de => (StatusCodes.Status409Conflict, Error(de)),
            EntityNotFoundException nf => (StatusCodes.Status404NotFound, Error(nf)),
            DomainException d => (StatusCodes.Status400BadRequest, Error(d)),
            ValidationException ve => (StatusCodes.Status400BadRequest, ValidationError(ve)),
            _ => (StatusCodes.Status500InternalServerError, Error("An unexpected error occurred."))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Unhandled exception");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static object Error(DomainException ex) => new { error = ex.Message };

    private static object Error(string message) => new { error = message };

    private static object ValidationError(ValidationException ex) => new
    {
        error = "Validation failed.",
        errors = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
    };
}

using System.Net;
using System.Text.Json;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;

namespace PoultryDistributionSystem.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, ILoggingService loggingService)
    {
        var requestId = context.TraceIdentifier;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, requestId, loggingService);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId, ILoggingService loggingService)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ApiResponse<object>();

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = ApiResponse<object>.ErrorResponse("Unauthorized access");
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = ApiResponse<object>.ErrorResponse(exception.Message);
                break;

            case ArgumentException:
            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.ErrorResponse(exception.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = ApiResponse<object>.ErrorResponse(
                    _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request");
                break;
        }

        // Log the exception
        await loggingService.LogErrorAsync(
            $"Error processing request: {exception.Message}",
            exception,
            requestId);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(json);
    }
}

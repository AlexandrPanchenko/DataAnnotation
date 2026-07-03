using JetFlight.Shared.Exceptions;
using Serilog;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace JetFlight.WebApi.Helpers;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AggregateException error)
        {
            var errorMessages = error.InnerExceptions.Select(e => e.Message);
            var joinedMessages = string.Join(", ", errorMessages);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, error, joinedMessages);
        }
        catch (ArgumentException error)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, error);
        }
        catch (BadRequestException error)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, error);
        }
        catch (NotFoundException error)
        {
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, error);
        }
        catch (Exception error)
        {
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, error);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, Exception error, string? errorMessage = null)
    {
        Log.Error("{Date} - {Error}", DateTime.UtcNow, error);
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var result = JsonSerializer.Serialize(new { message = errorMessage ?? error.Message }, options);
        await context.Response.WriteAsync(result);
    }
}
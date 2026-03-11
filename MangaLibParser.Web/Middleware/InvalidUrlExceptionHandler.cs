using MangaLibParser.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MangaLibParser.Web.Middleware;

public class InvalidUrlExceptionHandler(ILogger<InvalidUrlExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BaseArgumentException e)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Title = "Ошибка валидации URL",
            Detail = exception.Message,
            Status = (int)e.StatusCode,
        };

        httpContext.Response.StatusCode = (int)problemDetails.Status;
        logger.LogError(exception, "{ExceptionMessage}", exception.Message);
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
using MangaLibParser.Web.Middleware;

namespace MangaLibParser.Web;

public static class Extensions
{
    public static void AddExceptionsHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<InvalidUrlExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MangaLibParser.Web.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/users")
                           .WithTags("User");

        group.MapPost("/parse-library", ParseLibraryHandler);
        group.MapPost("/parse-library/download", DownloadLibraryHandler);
    }

    private static async Task<IResult> ParseLibraryHandler(
        [FromBody] ParseMangaRequest request,
        IUserLibrarySyncService syncService)
    {
        if (string.IsNullOrEmpty(request.Url))
        {
            return Results.BadRequest("URL профиля не может быть пустым");
        }

        try
        {
            var library = await syncService.ParseLibraryAsync(request.Url, request.Options);
            return Results.Ok(library);
        }
        catch (Exception e)
        {
            return Results.Problem("Ошибка при синхронизации библиотеки");
        }
    }

    private static async Task<IResult> DownloadLibraryHandler([FromBody] ParseMangaRequest request,
        IUserLibrarySyncService syncService)
    {
        if (string.IsNullOrEmpty(request.Url))
        {
            return Results.BadRequest("URL профиля не может быть пустым");
        }

        try
        {
            var zipFile = await syncService.ExportLibraryToZipAsync(request.Url, request.Options);

            return Results.File(
                zipFile,
                "application/zip",
                "library.zip"
            );
        }
        catch (Exception e)
        {
            return Results.Problem("Ошибка при создании архива библиотеки");
        }
    }
}
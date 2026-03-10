using System.Text;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MangaLibParser.Web.Endpoints;

public static class MangaEndpoints
{
    public static void MapMangaEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/manga")
                           .WithTags("Manga");

        group.MapPost("/parse", ParseMangaHandler);
        group.MapPost("/download/md", DownloadMarkdownHandler);
    }

    private static async Task<IResult> ParseMangaHandler(
        [FromBody] ParseMangaRequest request,
        IMangaInfoParserService parser)
    {
        // TODO Вынеси в глобальное перехватывание ошибок
        if (string.IsNullOrEmpty(request.Url))
        {
            return Results.BadRequest("URL манги не может быть пустым.");
        }

        try
        {
            var result = await parser.ParseMangaAsync(request.Url, request.Options);
            return Results.Ok(result);
        }
        catch (Exception e)
        {
            // TODO Логирование
            return Results.Problem($"Ошибка при парсинге: {e.Message}");
        }
    }

    private static async Task<IResult> DownloadMarkdownHandler([FromBody] ParseMangaRequest request,
        IMangaInfoParserService parserService,
        IMarkdownCreatorService markdownCreator)
    {
        var manga = await parserService.ParseMangaAsync(request.Url, request.Options);

        if (manga == null)
        {
            return Results.BadRequest("Не удалось получить данные о манге.");
        }

        var mdText = await markdownCreator.CreateMarkdown(manga, request.Options);
        var fileBytes = Encoding.UTF8.GetBytes(mdText);
        var name = $"{manga.TitleTranslated ?? Guid.NewGuid().ToString()}.md";

        return Results.File(
            fileBytes,
            "text/markdown",
            name
        );
    }
}
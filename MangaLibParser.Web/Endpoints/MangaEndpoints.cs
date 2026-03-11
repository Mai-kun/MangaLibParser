using System.Text;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.DTOs;
using MangaLibParser.Domain.Exceptions;
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
        if (CheckUrlRequest(request.Url))
        {
            throw new InvalidMangaUrlException();
        }

        var result = await parser.ParseMangaAsync(request.Url, request.Options);
        var dynamicResponse = MangaResponseMapper.ToDynamicResponse(result, request.Options);

        return Results.Ok(dynamicResponse);
    }

    private static async Task<IResult> DownloadMarkdownHandler([FromBody] ParseMangaRequest request,
        IMangaInfoParserService parserService,
        IMarkdownCreatorService markdownCreator)
    {
        if (CheckUrlRequest(request.Url))
        {
            throw new InvalidMangaUrlException();
        }

        var manga = await parserService.ParseMangaAsync(request.Url, request.Options);

        if (manga == null)
        {
            return Results.BadRequest("Не удалось получить данные о манге.");
        }

        var mdText = await markdownCreator.CreateMarkdown(manga, request.Options);
        var fileBytes = Encoding.UTF8.GetBytes(mdText);
        var name = $"{manga.TitleTranslated ?? manga.TitleOriginal ?? Guid.NewGuid().ToString()}.md";

        return Results.File(
            fileBytes,
            "text/markdown",
            name
        );
    }

    private static bool CheckUrlRequest(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return true;
        }

        if (!url.Contains("/manga/"))
        {
            return true;
        }

        return false;
    }
}
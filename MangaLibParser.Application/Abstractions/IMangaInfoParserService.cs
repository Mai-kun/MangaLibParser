using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Abstractions;

public interface IMangaInfoParserService
{
    /// <summary>
    ///     Parses manga information from the given URL.
    /// </summary>
    /// <param name="mangaUrl">The URL of the manga page.</param>
    /// <param name="options"></param>
    /// <returns>A <see cref="Manga" /> object containing the parsed information.</returns>
    public Task<Manga?> ParseMangaAsync(string mangaUrl, MangaParsingOptions options);
}
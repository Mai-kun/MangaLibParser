using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Abstractions;

public interface IMangaInfoParserService
{
    /// <summary>
    ///     Parses manga information from the given URL.
    /// </summary>
    /// <param name="url">The URL of the manga page.</param>
    /// <param name="options">The options for parsing the manga information.</param>
    /// <returns>A <see cref="Manga" /> object containing the parsed information.</returns>
    public Task<Manga> ParseMangaAsync(string url, MangaParsingOptions options);
}
using MangaLibParser.Application.Options;

namespace MangaLibParser.Application.DTOs;

public record ParseMangaRequest(
    string Url,
    MangaParsingOptions Options
);
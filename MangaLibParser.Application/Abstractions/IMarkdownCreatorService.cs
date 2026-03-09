using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Abstractions;

public interface IMarkdownCreatorService
{
    Task<string> CreateMarkdown(Manga manga, MangaParsingOptions options);
}
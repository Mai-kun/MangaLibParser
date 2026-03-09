using MangaLibParser.Application.Options;
using MangaLibParser.Application.Services;

namespace MangaLibParser.Application.Abstractions;

public interface IMarkdownPlanner
{
    MangaMarkdownPlan CreatePlan(MangaParsingOptions options);
}
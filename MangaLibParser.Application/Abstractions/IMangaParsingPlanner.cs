using MangaLibParser.Application.Options;

namespace MangaLibParser.Application.Abstractions;

public interface IMangaParsingPlanner
{
    MangaParsingPlan CreatePlan(MangaParsingOptions options);
}
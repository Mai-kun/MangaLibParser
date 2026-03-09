using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Infrastructure;

public class MarkdownCreatorService : IMarkdownCreatorService
{
    private readonly IMarkdownPlanner _planner;

    public MarkdownCreatorService(IMarkdownPlanner planner)
    {
        _planner = planner;
    }

    public Task<string> CreateMarkdown(Manga manga, MangaParsingOptions options)
    {
        var plan = _planner.CreatePlan(options);

        return Task.FromResult(plan.Execute(manga));
    }
}
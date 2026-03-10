using MangaLibParser.Domain.Entities;
using Microsoft.Playwright;

namespace MangaLibParser.Application.Options;

public class MangaParsingPlan
{
    public MangaParsingPlan(List<Func<IPage, string, Manga, Task>> steps)
    {
        Steps = steps;
    }

    public List<Func<IPage, string, Manga, Task>> Steps { get; }
}
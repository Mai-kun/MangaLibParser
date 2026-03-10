using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;
using Serilog;
using Serilog.Events;
using SerilogTracing;

namespace MangaLibParser.Infrastructure.Parsers;

public class MangaInfoParserService : IMangaInfoParserService
{
    private readonly PlaywrightBrowserManager _browserManager;
    private readonly ILogger _logger;
    private readonly IMangaParsingPlanner _mangaParsingPlanner;

    public MangaInfoParserService(PlaywrightBrowserManager browserManager, ILogger logger,
        IMangaParsingPlanner mangaParsingPlanner)
    {
        _browserManager = browserManager;
        _logger = logger;
        _mangaParsingPlanner = mangaParsingPlanner;
    }

    public async Task<Manga?> ParseMangaAsync(string mangaUrl, MangaParsingOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(mangaUrl);
        using var activity = _logger.StartActivity("Парсинг манги {MangaUrl}", mangaUrl);

        var plan = _mangaParsingPlanner.CreatePlan(options);

        var page = await _browserManager.GetNewPageAsync();
        try
        {
            await page.GotoAsync(mangaUrl);

            var manga = new Manga();
            var tasks = plan.Steps.Select(step => step(page, mangaUrl, manga));
            await Task.WhenAll(tasks);

            activity.Complete();
            return manga;
        }
        catch (Exception e)
        {
            activity.Complete(LogEventLevel.Error, e);
            return null;
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
using System.Text.Json;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Domain.Entities;
using Microsoft.Playwright;
using Serilog;
using SerilogTracing;

namespace MangaLibParser.Infrastructure.Parsers;

public class UserListParserService : IUserListParserService
{
    private readonly PlaywrightBrowserManager _browserManager;
    private readonly ILogger _logger;
    private IPage _page;

    public UserListParserService(PlaywrightBrowserManager browserManager, ILogger logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<List<UserMangaItem>> ParseUserListAsync(string userProfileUrl)
    {
        _page = await _browserManager.GetNewPageAsync();

        using var activity = _logger.StartActivity("Парсинг списка пользователя {UserProfileUrl}", userProfileUrl);

        await _page.GotoAsync(userProfileUrl);
        await AutoScrollAsync();

        var rawResult = await _page.EvaluateAsync<JsonElement>(@"() => {
            const container = document.querySelector('.book-list');
            if (!container) return [];

            const cards = container.querySelectorAll('[data-media-id]');
            
            return Array.from(cards).map(card => {
                const link = card.querySelector('a[href*=""/manga/""]');
                const rating = card.querySelector('[data-type=""positive""]');
                
                return {
                    Url: link ? link.href : '',
                    UserRating: rating ? parseInt(rating.textContent.trim()) : 0
                };
            });
        }");

        var result = JsonSerializer.Deserialize<List<UserMangaItem>>(rawResult.GetRawText());

        activity.Complete();
        return result ?? [];
    }

    private async Task AutoScrollAsync()
    {
        while (true)
        {
            var previousHeight = await _page.EvaluateAsync<int>("document.body.scrollHeight");

            await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");

            await _page.WaitForTimeoutAsync(2000);

            var newHeight = await _page.EvaluateAsync<int>("document.body.scrollHeight");

            if (newHeight == previousHeight)
            {
                break;
            }
        }
    }
}
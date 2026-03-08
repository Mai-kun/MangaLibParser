using System.Text.Json;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Domain.Entities;
using Microsoft.Playwright;

namespace MangaLibParser.Infrastructure.Parsers;

public class UserListParserService : IUserListParserService
{
    private readonly PlaywrightBrowserManager _browserManager;
    private IPage _page;

    public UserListParserService(PlaywrightBrowserManager browserManager)
    {
        _browserManager = browserManager;
    }

    public async Task<List<UserMangaItem>> ParseUserListAsync(long userId, long listType)
    {
        _page = await _browserManager.GetNewPageAsync();

        var url = $"https://mangalib.me/ru/user/{userId}?page=1&sort_by=updated_at&sort_type=desc&status={listType}";
        await _page.GotoAsync(url);
        await AutoScrollAsync();

        var rawResult = await _page.EvaluateAsync<JsonElement>(@"() => {
            const container = document.querySelector('div[data-view=""list""]');
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
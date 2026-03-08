using System.ComponentModel;
using System.Text.RegularExpressions;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;
using Microsoft.Playwright;

namespace MangaLibParser.Infrastructure.Parsers;

public class MangaInfoParserService : IMangaInfoParserService
{
    private readonly PlaywrightBrowserManager _browserManager;

    public MangaInfoParserService(PlaywrightBrowserManager browserManager)
    {
        _browserManager = browserManager;
    }

    public async Task<Manga> ParseMangaAsync(string url, MangaParsingOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        var page = await _browserManager.GetNewPageAsync();
        try
        {
            await page.GotoAsync(url);

            var manga = new Manga
            {
                Url = options.ParseUrl
                    ? url
                    : null,

                TitleTranslated = options.ParseTitleTranslated
                    ? await GetTitleContentAsync(1, page)
                    : null,

                TitleOriginal = options.ParseTitleOriginal
                    ? await GetTitleContentAsync(2, page)
                    : null,

                Description = options.ParseDescription
                    ? await GetContentBySelectorAsync<string>(".text-collapse", page)
                    : null,

                AgeRating = options.ParseAgeRating
                    ? await GetContentBySelectorAsync<int>("a[data-type='restriction']", page)
                    : null,

                Cover = options.ParseCover
                    ? await page.Locator("img[src*='/cover/']").First.GetAttributeAsync("src")
                    : null,

                ReleaseYear = options.ParseReleaseYear
                    ? await GetContentBySelectorAsync<int>("a:has-text('Выпуск') span", page, @"\d+")
                    : null,

                TranslationStatus = options.ParseTranslationStatus
                    ? await GetContentBySelectorAsync<string>("a:has-text('Перевод') span", page)
                    : null,

                Type = options.ParseType
                    ? await GetContentBySelectorAsync<string>("a:has-text('Тип') span", page)
                    : null,

                GeneralRating = options.ParseGeneralRating
                    ? await GetContentBySelectorAsync<float>(".rating-info__value", page)
                    : null,

                ChaptersAmount = options.ParseChaptersAmount
                    ? await GetContentBySelectorAsync<int>("div:text-is('Глав') + div span", page, @"\d+")
                    : null,

                UserRating = options.ParseUserRating
                    ? await GetContentBySelectorAsync<int>(".rating-info__value", page)
                    : null,

                ReleaseStatus = options.ParseReleaseStatus
                    ? await GetContentBySelectorAsync<string>("a:has-text('Статус') span", page)
                    : null,

                Authors = options.ParseAuthors
                    ? await GetListContentAsync("a[href*='/people/']", page)
                    : [],

                Genres = options.ParseGenres
                    ? await GetListContentAsync("a[data-type='genre']", page)
                    : [],

                Tags = options.ParseTags
                    ? await GetListContentAsync("a[data-type='tag']", page)
                    : [],

                Publishers = options.ParsePublishers
                    ? await GetListContentAsync("a[href*='/publisher/']", page)
                    : [],

                Translators = options.ParseTranslators
                    ? await GetListContentAsync("a[href*='/team/']", page)
                    : [],
            };

            return manga;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static async Task<string?> GetTitleContentAsync(int level, IPage page)
    {
        var headingLocator = page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Level = level }).First;
        var headingContent = await headingLocator.TextContentAsync();
        return headingContent;
    }

    private static async Task<T?> GetContentBySelectorAsync<T>(string selector, IPage page, string? regexPattern = null)
    {
        var locator = page.Locator(selector);
        return await ExtractContentAsync<T>(locator, regexPattern);
    }

    private static async Task<T?> ExtractContentAsync<T>(ILocator locator, string? regexPattern = null)
    {
        var locatorContent = await locator.First.TextContentAsync();
        var cleanContent = locatorContent?.Trim();

        if (string.IsNullOrEmpty(cleanContent))
        {
            return default;
        }

        if (!string.IsNullOrEmpty(regexPattern))
        {
            var match = Regex.Match(cleanContent, regexPattern);
            if (match.Success)
            {
                cleanContent = match.Value;
            }
            else
            {
                return default;
            }
        }

        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T?)converter.ConvertFromString(cleanContent);
        }
        catch
        {
            return default;
        }
    }

    private static async Task<List<string>> GetListContentAsync(string selector, IPage page)
    {
        var locators = await page.Locator(selector).AllTextContentsAsync();
        return locators.Any()
            ? locators.Select(s => s.Trim()).ToList()
            : [];
    }
}
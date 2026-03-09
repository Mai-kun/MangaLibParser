using System.ComponentModel;
using System.Text.RegularExpressions;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;
using Microsoft.Playwright;
using Serilog;
using Serilog.Events;
using SerilogTracing;

namespace MangaLibParser.Infrastructure.Parsers;

public class MangaInfoParserService : IMangaInfoParserService
{
    private readonly PlaywrightBrowserManager _browserManager;
    private readonly ILogger _logger;
    private readonly IMarkdownPlanner _markdownPlanner;

    public MangaInfoParserService(PlaywrightBrowserManager browserManager, ILogger logger,
        IMarkdownPlanner markdownPlanner)
    {
        _browserManager = browserManager;
        _logger = logger;
        _markdownPlanner = markdownPlanner;
    }

    public async Task<Manga?> ParseMangaAsync(string mangaUrl, MangaParsingOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(mangaUrl);

        using var activity = _logger.StartActivity("Парсинг манги {MangaUrl}", mangaUrl);

        var page = await _browserManager.GetNewPageAsync();
        try
        {
            await page.GotoAsync(mangaUrl);

            var manga = new Manga
            {
                Url = options.ParseUrl
                    ? mangaUrl
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
                    ? await GetContentBySelectorAsync<int>("a[data-type='restriction']", page, @"\d+")
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
                    ? await GetContentBySelectorAsync<float>(".rating-info__value", page, @"[\d\.]+")
                    : null,

                ChaptersAmount = options.ParseChaptersAmount
                    ? await GetContentBySelectorAsync<int>("div:text-is('Глав') + div span", page, @"\d+")
                    : null,

                // UserRating is not directly parseable from the main page as it's often dynamic or requires user context.
                // Setting it to null as per current design.
                UserRating = null,

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

    private async Task<string?> GetTitleContentAsync(int level, IPage page)
    {
        using var activity = _logger.StartActivity("Получение заголовка уровня {Level}", level);

        var headingLocator = page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Level = level }).First;
        var headingContent = await headingLocator.TextContentAsync();

        activity.Complete();
        return headingContent;
    }

    private async Task<T?> GetContentBySelectorAsync<T>(string selector, IPage page, string? regexPattern = null)
    {
        try
        {
            var locator = page.Locator(selector);
            return await ExtractContentAsync<T>(locator, selector, regexPattern);
        }
        catch (TimeoutException e)
        {
            _logger.Warning(e, "Timeout while trying to locate element with selector '{Selector}'", selector);
            return default;
        }
    }

    private async Task<T?> ExtractContentAsync<T>(ILocator locator, string selector, string? regexPattern = null)
    {
        using var activity = _logger.StartActivity("Получение селектора {Selector}", selector);

        try
        {
            var locatorContent =
                await locator.First.TextContentAsync(new LocatorTextContentOptions { Timeout = 2000 });
            var cleanContent = locatorContent?.Trim();

            if (string.IsNullOrEmpty(cleanContent))
            {
                _logger.Debug("No content found for selector '{Selector}' or content was empty/whitespace.", selector);
                return default;
            }

            if (!string.IsNullOrEmpty(regexPattern))
            {
                var match = Regex.Match(cleanContent, regexPattern);
                if (match.Success)
                {
                    cleanContent = match.Value;
                    _logger.Debug("Content for selector '{Selector}' matched regex '{RegexPattern}': {CleanContent}",
                        selector, regexPattern, cleanContent);
                }
                else
                {
                    _logger.Warning(
                        "Content '{Content}' for selector '{Selector}' did not match regex '{RegexPattern}'.",
                        cleanContent, selector, regexPattern);
                    return default;
                }
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            var convertedValue = (T?)converter.ConvertFromString(cleanContent);

            _logger.Debug("Successfully converted content '{CleanContent}' to type {Type} for selector '{Selector}'.",
                cleanContent, typeof(T).Name, selector);

            activity.Complete();
            return convertedValue;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to extract or convert content for {Selector}", selector);
            activity.Complete(LogEventLevel.Error, e);
            return default;
        }
    }

    private async Task<List<string>> GetListContentAsync(string selector, IPage page)
    {
        using var activity = _logger.StartActivity("Получение списка {Selector}", selector);

        try
        {
            var locators = await page.Locator(selector).AllTextContentsAsync();
            if (locators.Any())
            {
                var trimmedLocators = locators.Select(s => s.Trim()).ToList();
                _logger.Debug("Extracted list content for selector '{Selector}': {Content}", selector, trimmedLocators);

                activity.Complete();
                return trimmedLocators;
            }

            _logger.Debug("No list content found for selector '{Selector}'.", selector);
            activity.Complete();
            return [];
        }
        catch (Exception e)
        {
            activity.Complete(LogEventLevel.Error, e);
            return [];
        }
    }
}
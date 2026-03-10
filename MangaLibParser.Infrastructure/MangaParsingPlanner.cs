using System.ComponentModel;
using System.Text.RegularExpressions;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;
using Microsoft.Playwright;
using Serilog;
using Serilog.Events;
using SerilogTracing;

namespace MangaLibParser.Infrastructure;

public class MangaParsingPlanner : IMangaParsingPlanner
{
    private readonly ILogger _logger;

    public MangaParsingPlanner(ILogger logger)
    {
        _logger = logger;
    }

    public MangaParsingPlan CreatePlan(MangaParsingOptions options)
    {
        var steps = new List<Func<IPage, string, Manga, Task>>();

        if (options.ParseUrl)
        {
            steps.Add((_, url, manga) => Task.FromResult(manga.Url = url));
        }

        if (options.ParseTitleTranslated)
        {
            steps.Add(async (page, _, manga) => manga.TitleTranslated = await GetTitleContentAsync(1, page));
        }

        if (options.ParseTitleOriginal)
        {
            steps.Add(async (page, _, manga) => manga.TitleOriginal = await GetTitleContentAsync(2, page));
        }

        if (options.ParseDescription)
        {
            steps.Add(async (page, _, manga) =>
                manga.Description = await GetContentBySelectorAsync<string>(".text-collapse", page));
        }

        if (options.ParseAgeRating)
        {
            steps.Add(async (page, _, manga) => manga.AgeRating =
                await GetContentBySelectorAsync<int>("a[data-type='restriction']", page, @"\d+"));
        }

        if (options.ParseCover)
        {
            steps.Add(async (page, _, manga) =>
                manga.Cover = await page.Locator("img[src*='/cover/']").First.GetAttributeAsync("src"));
        }

        if (options.ParseReleaseYear)
        {
            steps.Add(async (page, _, manga) => manga.ReleaseYear =
                await GetContentBySelectorAsync<int>("a:has-text('Выпуск') span", page, @"\d+"));
        }

        if (options.ParseTranslationStatus)
        {
            steps.Add(async (page, _, manga) => manga.TranslationStatus =
                await GetContentBySelectorAsync<string>("a:has-text('Перевод') span", page));
        }

        if (options.ParseType)
        {
            steps.Add(async (page, _, manga) =>
                manga.Type = await GetContentBySelectorAsync<string>("a:has-text('Тип') span", page));
        }

        if (options.ParseGeneralRating)
        {
            steps.Add(async (page, _, manga) => manga.GeneralRating =
                await GetContentBySelectorAsync<float>(".rating-info__value", page, @"[\d\.]+"));
        }

        if (options.ParseChaptersAmount)
        {
            steps.Add(async (page, _, manga) => manga.ChaptersAmount =
                await GetContentBySelectorAsync<int>("div:text-is('Глав') + div span", page, @"\d+"));
        }

        if (options.ParseReleaseStatus)
        {
            steps.Add(async (page, _, manga) => manga.ReleaseStatus =
                await GetContentBySelectorAsync<string>("a:has-text('Статус') span", page));
        }

        if (options.ParseAuthors)
        {
            steps.Add(async (page, _, manga) =>
                manga.Authors = await GetListContentAsync("a[href*='/people/']", page));
        }

        if (options.ParseGenres)
        {
            steps.Add(async (page, _, manga) =>
                manga.Genres = await GetListContentAsync("a[data-type='genre']", page));
        }

        if (options.ParseTags)
        {
            steps.Add(async (page, _, manga) => manga.Tags = await GetListContentAsync("a[data-type='tag']", page));
        }

        if (options.ParsePublishers)
        {
            steps.Add(async (page, _, manga) =>
                manga.Publishers = await GetListContentAsync("a[href*='/publisher/']", page));
        }

        if (options.ParseTranslators)
        {
            steps.Add(async (page, _, manga) =>
                manga.Translators = await GetListContentAsync("a[href*='/team/']", page));
        }

        return new MangaParsingPlan(steps);
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
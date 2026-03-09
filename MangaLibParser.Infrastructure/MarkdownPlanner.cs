using System.Text;
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Application.Services;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Infrastructure;

public class MarkdownPlanner : IMarkdownPlanner
{
    public MangaMarkdownPlan CreatePlan(MangaParsingOptions options)
    {
        var actions = new List<Action<StringBuilder, Manga>>();

        AppendDelimiter(actions);

        if (options.ParseUrl)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-url: {m.Url}"));
        }

        if (options.ParseTitleTranslated)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-title-translated: {m.TitleTranslated}"));
        }

        if (options.ParseTitleOriginal)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-title-original: {m.TitleOriginal}"));
        }

        if (options.ParseCover)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-cover: {m.Cover}"));
        }

        if (options.ParseType)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-type: {m.Type}"));
        }

        if (options.ParseReadingStatus)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-reading-status: {m.ReadingStatus}"));
        }

        if (options.ParseReleaseStatus)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-release-status: {m.ReleaseStatus}"));
        }

        if (options.ParseTranslationStatus)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-translation-status: {m.TranslationStatus}"));
        }

        if (options.ParseAgeRating)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-age-rating: {m.AgeRating}"));
        }

        if (options.ParseUserRating)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-user-rating: {m.UserRating}"));
        }

        if (options.ParseGeneralRating)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-general-rating: {m.GeneralRating}"));
        }

        if (options.ParseReleaseYear)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-release-year: {m.ReleaseYear}"));
        }

        if (options.ParseChaptersAmount)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-chapters-amount: {m.ChaptersAmount}"));
        }

        if (options.ParseGenres)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-genres: {string.Join(", ", m.Genres)}"));
        }

        if (options.ParseTags)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-tags: {string.Join(", ", m.Tags)}"));
        }

        if (options.ParseAuthors)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-authors: {string.Join(", ", m.Authors)}"));
        }

        if (options.ParsePublishers)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-publishers: {string.Join(", ", m.Publishers)}"));
        }

        if (options.ParseTranslators)
        {
            actions.Add((sb, m) => sb.AppendLine($"book-translators: {string.Join(", ", m.Translators)}"));
        }

        AppendDelimiter(actions);

        if (options.ParseDescription)
        {
            actions.Add((sb, m) =>
                sb.AppendLine($"## Описание {Environment.NewLine + Environment.NewLine + m.Description}"));
        }

        return new MangaMarkdownPlan(actions);
    }

    private static void AppendDelimiter(List<Action<StringBuilder, Manga>> actions)
    {
        actions.Add((sb, m) => sb.AppendLine("---"));
    }
}
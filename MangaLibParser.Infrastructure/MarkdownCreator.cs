using System.Text;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Infrastructure;

public class MarkdownCreator
{
    public async Task CreateMarkdown(Manga manga, MangaParsingOptions options)
    {
        StringBuilder builder = new();

        if (options.ParseTitleTranslated)
        {
            builder.Append("manga-title: " + manga.TitleTranslated + Environment.NewLine);
        }

        if (options.ParseTitleOriginal)
        {
            builder.Append("manga-title-original: " + manga.TitleOriginal + Environment.NewLine);
        }

        if (options.ParseAuthors)
        {
            builder.Append("manga-author: " +
                           string.Join($"{Environment.NewLine} - ", manga.Authors) + Environment.NewLine);
        }

        if (options.ParseCover)
        {
            builder.Append("manga-cover: " + manga.Cover + Environment.NewLine);
        }

        if (options.ParseReadingStatus)
        {
            builder.Append("manga-reading-status: " + manga.ReadingStatus + Environment.NewLine);
        }

        if (options.ParseReleaseStatus)
        {
            builder.Append("manga-release-status: " + manga.ReleaseStatus + Environment.NewLine);
        }

        if (options.ParseTranslationStatus)
        {
            builder.Append("manga-translation-status: " + manga.TranslationStatus + Environment.NewLine);
        }

        if (options.ParseUrl)
        {
            builder.Append("manga-url: " + manga.Url + Environment.NewLine);
        }

        if (options.ParseDescription)
        {
            builder.Append("manga-description: " + manga.Description + Environment.NewLine);
        }

        if (options.ParseAgeRating)
        {
            builder.Append("manga-age-rating: " + manga.AgeRating + Environment.NewLine);
        }

        if (options.ParseGenres)
        {
            builder.Append("manga-genres: " +
                           string.Join($"{Environment.NewLine} - ", manga.Genres) + Environment.NewLine);
        }

        if (options.ParseTags)
        {
            builder.Append("manga-tags: " +
                           string.Join($"{Environment.NewLine} - ", manga.Tags) + Environment.NewLine);
        }

        if (options.ParseUserRating && manga.UserRating != 0)
        {
            builder.Append("manga-user-rating: " + manga.UserRating + Environment.NewLine);
        }

        if (options.ParseGeneralRating)
        {
            builder.Append("manga-general-rating: " + manga.GeneralRating + Environment.NewLine);
        }

        if (options.ParseType)
        {
            builder.Append("manga-type: " + manga.Type + Environment.NewLine);
        }

        if (options.ParseReleaseYear)
        {
            builder.Append("manga-release-year: " + manga.ReleaseYear + Environment.NewLine);
        }

        if (options.ParseChaptersAmount)
        {
            builder.Append("manga-chapters-amount: " + manga.ChaptersAmount + Environment.NewLine);
        }

        if (options.ParsePublishers)
        {
            builder.Append("manga-publishers: " + string.Join(", ", manga.Publishers) + Environment.NewLine);
        }

        if (options.ParseTranslators)
        {
            builder.Append("manga-translators: " + string.Join(", ", manga.Translators) + Environment.NewLine);
        }

        await File.WriteAllTextAsync($"{manga.TitleTranslated}.md", encoding: Encoding.UTF8,
            contents: builder.ToString());
    }
}
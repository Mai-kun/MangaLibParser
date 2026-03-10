namespace MangaLibParser.Domain.Entities;

public class Manga
{
    /// <summary>
    ///     The URL of the manga.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     The translated title of the manga.
    /// </summary>
    public string? TitleTranslated { get; set; }

    /// <summary>
    ///     The original title of the manga.
    /// </summary>
    public string? TitleOriginal { get; set; }

    /// <summary>
    ///     The description of the manga.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The URL of the manga's cover image.
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    ///     The type of the manga (e.g., "Manga", "Manhwa").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     The reading status of the manga (e.g., "Reading", "Completed").
    /// </summary>
    public string? ReadingStatus { get; set; }

    /// <summary>
    ///     The release status of the manga (e.g., "Ongoing", "Finished").
    /// </summary>
    public string? ReleaseStatus { get; set; }

    /// <summary>
    ///     The translation status of the manga.
    /// </summary>
    public string? TranslationStatus { get; set; }

    /// <summary>
    ///     The age rating of the manga.
    /// </summary>
    public int? AgeRating { get; set; }

    /// <summary>
    ///     The user's rating for the manga.
    /// </summary>
    public int? UserRating { get; set; }

    /// <summary>
    ///     The general rating of the manga.
    /// </summary>
    public float? GeneralRating { get; set; }

    /// <summary>
    ///     The release year of the manga.
    /// </summary>
    public int? ReleaseYear { get; set; }

    /// <summary>
    ///     The total number of chapters in the manga.
    /// </summary>
    public int? ChaptersAmount { get; set; }

    /// <summary>
    ///     A list of genres associated with the manga.
    /// </summary>
    public List<string> Genres { get; set; } = [];

    /// <summary>
    ///     A list of tags associated with the manga.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    ///     A list of authors of the manga.
    /// </summary>
    public List<string> Authors { get; set; } = [];

    /// <summary>
    ///     A list of publishers of the manga.
    /// </summary>
    public List<string> Publishers { get; set; } = [];

    /// <summary>
    ///     A list of translators of the manga.
    /// </summary>
    public List<string> Translators { get; set; } = [];
}
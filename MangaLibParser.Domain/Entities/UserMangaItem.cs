namespace MangaLibParser.Domain.Entities;

public class UserMangaItem
{
    /// <summary>
    ///     The URL of the manga.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     The user's rating for the manga.
    /// </summary>
    public int? UserRating { get; set; }
}
namespace MangaLibParser.Application.Options;

public class MangaParsingOptions
{
    public bool ParseUrl { get; set; } = true;
    public bool ParseTitleTranslated { get; set; } = true;
    public bool ParseTitleOriginal { get; set; } = true;
    public bool ParseDescription { get; set; } = true;
    public bool ParseCover { get; set; } = true;
    public bool ParseAgeRating { get; set; } = true;
    public bool ParseGenres { get; set; } = true;
    public bool ParseTags { get; set; } = true;
    public bool ParseUserRating { get; set; } = true;
    public bool ParseGeneralRating { get; set; } = true;
    public bool ParseType { get; set; } = true;
    public bool ParseReleaseYear { get; set; } = true;
    public bool ParseChaptersAmount { get; set; } = true;
    public bool ParseReadingStatus { get; set; } = true;
    public bool ParseReleaseStatus { get; set; } = true;
    public bool ParseTranslationStatus { get; set; } = true;
    public bool ParseAuthors { get; set; } = true;
    public bool ParsePublishers { get; set; } = true;
    public bool ParseTranslators { get; set; } = true;
}
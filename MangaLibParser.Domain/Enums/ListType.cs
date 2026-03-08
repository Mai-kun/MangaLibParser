namespace MangaLibParser.Domain.Enums;

/// <summary>
///     Represents different types of manga lists a user can have.
/// </summary>
public enum ListType
{
    /// <summary>
    ///     Represents manga that the user is currently reading.
    /// </summary>
    Reading = 1,

    /// <summary>
    ///     Represents manga that the user plans to read.
    /// </summary>
    InPlan = 2,

    /// <summary>
    ///     Represents manga that the user has dropped.
    /// </summary>
    Dropped = 3,

    /// <summary>
    ///     Represents manga that the user has completed.
    /// </summary>
    Completed = 4,

    /// <summary>
    ///     Represents manga that the user has marked as favorites.
    /// </summary>
    Favorites = 5,
}
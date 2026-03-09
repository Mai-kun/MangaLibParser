using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Abstractions;

public interface IUserLibrarySyncService
{
    public Task<List<Manga>> SyncLibraryAsync(string userProfileUrl, MangaParsingOptions options);
}
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Abstractions;

public interface IUserLibrarySyncService
{
    public Task<byte[]> ExportLibraryToZipAsync(string userProfileUrl, MangaParsingOptions options);
    public Task<List<Manga>> ParseLibraryAsync(string userProfileUrl, MangaParsingOptions options);
}
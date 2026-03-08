using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Abstractions;

public interface IUserListParserService
{
    public Task<List<UserMangaItem>> ParseUserListAsync(long userId, long listType);
}
using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;
using Serilog;
using SerilogTracing;

namespace MangaLibParser.Application.Services;

public class UserLibrarySyncService : IUserLibrarySyncService
{
    private readonly ILogger _logger;
    private readonly IMangaInfoParserService _mangaInfoParserService;
    private readonly IUserListParserService _userListParserService;

    public UserLibrarySyncService(IUserListParserService userListParserService,
        IMangaInfoParserService mangaInfoParserService, ILogger logger)
    {
        _userListParserService = userListParserService;
        _mangaInfoParserService = mangaInfoParserService;
        _logger = logger;
    }

    public async Task<List<Manga>> SyncLibraryAsync(string userProfileUrl, MangaParsingOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(userProfileUrl);

        using var activity =
            _logger.StartActivity("Синхронизация библиотеки пользователя {UserProfileUrl}", userProfileUrl);

        var userMangasList = await _userListParserService.ParseUserListAsync(userProfileUrl);

        var resultList = new List<Manga>();
        foreach (var mangaItem in userMangasList)
        {
            try
            {
                var manga = await _mangaInfoParserService.ParseMangaAsync(mangaItem.Url, options);
                resultList.Add(manga);
            }
            catch (Exception e)
            {
                // TODO Логирование
                _logger.Error(e, "Ошибка при парсинге манги {MangaUrl}", mangaItem.Url);
            }
        }

        activity.Complete();
        return resultList;
    }
}
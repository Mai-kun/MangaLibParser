using System.IO.Compression;
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
    private readonly IMarkdownPlanner _planner;
    private readonly Random _random = new();
    private readonly IUserListParserService _userListParserService;

    public UserLibrarySyncService(IUserListParserService userListParserService,
        IMangaInfoParserService mangaInfoParserService, ILogger logger, IMarkdownPlanner planner)
    {
        _userListParserService = userListParserService;
        _mangaInfoParserService = mangaInfoParserService;
        _logger = logger;
        _planner = planner;
    }

    public async Task<byte[]> ExportLibraryToZipAsync(string userProfileUrl, MangaParsingOptions options)
    {
        var plan = _planner.CreatePlan(options);
        var mangaList = await _userListParserService.ParseUserListAsync(userProfileUrl);

        using var memoryStream = new MemoryStream();
        await using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var manga in mangaList)
            {
                var mangaInfo = await _mangaInfoParserService.ParseMangaAsync(manga.Url, options);
                if (mangaInfo is null)
                {
                    continue;
                }

                var content = plan.Execute(mangaInfo);

                var fileName = $"{mangaInfo.TitleTranslated ?? mangaInfo.TitleOriginal ?? $"{Guid.NewGuid()}"}.md";
                var entry = archive.CreateEntry(fileName);

                await using var entryStream = await entry.OpenAsync();
                await using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(content);

                await Task.Delay(_random.Next(2000, 5000));
            }
        }

        return memoryStream.ToArray();
    }

    public async Task<List<Manga>> ParseLibraryAsync(string userProfileUrl, MangaParsingOptions options)
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

                await Task.Delay(_random.Next(2000, 5000));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при парсинге манги {MangaUrl}", mangaItem.Url);
            }
        }

        activity.Complete();
        return resultList;
    }
}
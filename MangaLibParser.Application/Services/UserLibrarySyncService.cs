using System.IO.Compression;
using System.Text;
using System.Web;
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
        ArgumentException.ThrowIfNullOrEmpty(userProfileUrl);
        var plan = _planner.CreatePlan(options);

        var userMangasList = await _userListParserService.ParseUserListAsync(userProfileUrl);
        var status = GetStatusFromUrl(userProfileUrl);

        using var memoryStream = new MemoryStream();
        await using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var mangaItem in userMangasList)
            {
                try
                {
                    var manga = await GetFullMangaInfo(options, mangaItem, status);

                    var content = plan.Execute(manga);
                    var baseName = manga.TitleTranslated ?? manga.TitleOriginal ?? Guid.NewGuid().ToString();
                    var fileName = $"{GetSafeFileName(baseName)}.md";
                    var entry = archive.CreateEntry(fileName);
                    await using (var entryStream = await entry.OpenAsync())
                    await using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                    {
                        await writer.WriteAsync(content);
                    }

                    await Task.Delay(_random.Next(1500, 3000));
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Пропущен экспорт манги {Url}", mangaItem.Url);
                }
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
        var status = GetStatusFromUrl(userProfileUrl);

        var resultList = new List<Manga>();
        foreach (var mangaItem in userMangasList)
        {
            try
            {
                var manga = await GetFullMangaInfo(options, mangaItem, status);
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

    private static string? GetStatusFromUrl(string userProfileUrl)
    {
        try
        {
            var uri = new Uri(userProfileUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var status = query["status"];
            return status;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Manga> GetFullMangaInfo(MangaParsingOptions options, UserMangaItem mangaItem, string? status)
    {
        var mangaResult = new Manga { Url = mangaItem.Url };

        if (string.IsNullOrEmpty(mangaItem.Url))
        {
            _logger.Warning("В списке встречен пустой URL манги");
            return mangaResult;
        }

        var detailedManga = await _mangaInfoParserService.ParseMangaAsync(mangaItem.Url, options);
        if (detailedManga != null)
        {
            mangaResult = detailedManga;
        }

        mangaResult.UserRating = mangaItem.UserRating;
        mangaResult.ReadingStatus = status;

        return mangaResult;
    }

    private static string GetSafeFileName(string name)
    {
        if (name.Length > 150)
        {
            name = name[..150];
        }

        return Path.GetInvalidFileNameChars()
                   .Aggregate(name, (current, c) => current.Replace(c, '_'));
    }
}
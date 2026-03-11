using MangaLibParser.Application.Options;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.DTOs;

public static class MangaResponseMapper
{
    public static Dictionary<string, object?> ToDynamicResponse(Manga manga, MangaParsingOptions options)
    {
        var result = new Dictionary<string, object?>();

        var mangaProps = typeof(Manga).GetProperties();

        foreach (var optProp in typeof(MangaParsingOptions).GetProperties())
        {
            if (optProp.PropertyType == typeof(bool) && (bool)optProp.GetValue(options)!)
            {
                var targetName = optProp.Name.Replace("Parse", "");
                var mangaProp = mangaProps.FirstOrDefault(p => p.Name == targetName);

                if (mangaProp != null)
                {
                    var jsonKey = char.ToLower(targetName[0]) + targetName[1..];

                    result[jsonKey] = mangaProp.GetValue(manga);
                }
            }
        }

        return result;
    }
}
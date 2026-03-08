using System.Text.Json;
using System.Text.Json.Serialization;

namespace MangaLibParser.Infrastructure;

public static class Deserializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static async Task<T?> DeserializeAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл {filePath} не найден");
        }

        await using var stream = File.OpenRead(filePath);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, Options);
        return result ?? default;
    }
}
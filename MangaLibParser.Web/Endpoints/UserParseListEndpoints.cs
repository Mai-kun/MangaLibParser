using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MangaLibParser.Web.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/users")
                           .WithTags("User");

        group.MapPost("/parse", ParseUserListHandler);
    }

    private static async Task<IResult> ParseUserListHandler(
        [FromBody] ParseUserListRequest request,
        IUserListParserService parser)
    {
        // TODO Вынеси в глобальное перехватывание ошибок
        if (request.UserId == 0)
        {
            return Results.BadRequest("Id пользователя должно быть указано");
        }

        try
        {
            var result = await parser.ParseUserListAsync(request.UserId, request.ListType);
            return Results.Ok(result);
        }
        catch (Exception e)
        {
            // TODO Логирование
            return Results.Problem($"Ошибка при парсинге: {e.Message}");
        }
    }
}
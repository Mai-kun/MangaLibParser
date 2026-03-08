namespace MangaLibParser.Application.DTOs;

public record ParseUserListRequest(
    long UserId,
    long ListType
);
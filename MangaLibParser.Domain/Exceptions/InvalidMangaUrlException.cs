namespace MangaLibParser.Domain.Exceptions;

public class InvalidMangaUrlException()
    : BaseArgumentException("Неверный формат URL манги.")
{
}
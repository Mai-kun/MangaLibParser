namespace MangaLibParser.Domain.Exceptions;

public class InvalidUserUrlException()
    : BaseArgumentException("Неверный URL манги. URL должен содержать сегмент '/manga/'.")
{
}
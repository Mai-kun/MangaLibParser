using System.Net;

namespace MangaLibParser.Domain.Exceptions;

public class BaseArgumentException(string message)
    : BaseException(message, HttpStatusCode.BadRequest)
{
}
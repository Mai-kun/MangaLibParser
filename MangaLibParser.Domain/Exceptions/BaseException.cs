using System.Net;

namespace MangaLibParser.Domain.Exceptions;

public class BaseException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
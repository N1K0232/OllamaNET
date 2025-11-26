namespace OllamaNET.Exceptions;

public class OllamaClientException(string? message, int statusCode, Exception? innerException) : Exception(message, innerException)
{
    public int StatusCode { get; } = statusCode;

    public OllamaClientException(string message, int statusCode) : this(message, statusCode, null)
    {
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OllamaNET.Exceptions;

public class EmbeddingException(string? message, int statusCode, Exception? innerException) : Exception(message, innerException)
{
    public int StatusCode { get; } = statusCode;

    public EmbeddingException(string message, int statusCode) : this(message, statusCode, null)
    {
    }
}
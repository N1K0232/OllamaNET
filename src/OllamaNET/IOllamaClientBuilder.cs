using Microsoft.Extensions.DependencyInjection;

namespace OllamaNET;

public interface IOllamaClientBuilder
{
    IServiceCollection Services { get; }

    IHttpClientBuilder HttpClientBuilder { get; }
}
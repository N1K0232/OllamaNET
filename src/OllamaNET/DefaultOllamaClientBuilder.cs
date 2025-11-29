using Microsoft.Extensions.DependencyInjection;

namespace OllamaNET;

internal class DefaultOllamaClientBuilder : IOllamaClientBuilder
{
    internal DefaultOllamaClientBuilder(IServiceCollection services, IHttpClientBuilder httpClientBuilder)
    {
        Services = services;
        HttpClientBuilder = httpClientBuilder;
    }

    public IServiceCollection Services { get; }

    public IHttpClientBuilder HttpClientBuilder { get; }
}
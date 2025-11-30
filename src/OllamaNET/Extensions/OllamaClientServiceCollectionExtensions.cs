using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OllamaNET.Caching;
using Polly;

namespace OllamaNET.Extensions;

public static class OllamaClientServiceCollectionExtensions
{
    public static IOllamaClientBuilder AddOllamaClient(this IServiceCollection services, Action<OllamaClientOptions>? setupAction = null)
    {
        var options = new OllamaClientOptions();
        setupAction?.Invoke(options);

        services.AddSingleton(options);

        var httpClientBuilder = services.AddHttpClient<IOllamaClient, OllamaClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(options.ServiceUrl);
        });

        return new DefaultOllamaClientBuilder(services, httpClientBuilder);
    }

    public static IOllamaClientBuilder AddOllamaClient(this IServiceCollection services, IConfiguration configuration, string sectionName = "Ollama")
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName, nameof(sectionName));

        var options = configuration.GetSection(sectionName).Get<OllamaClientOptions>() ?? new OllamaClientOptions();
        services.AddSingleton(options);

        var httpClientBuilder = services.AddHttpClient<IOllamaClient, OllamaClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(options.ServiceUrl);
        });

        return new DefaultOllamaClientBuilder(services, httpClientBuilder);
    }

    public static IOllamaClientBuilder WithMemoryCache(this IOllamaClientBuilder builder, Action<MemoryCacheOptions>? setupAction = null)
    {
        if (setupAction is null)
        {
            builder.Services.AddMemoryCache();
        }
        else
        {
            builder.Services.AddMemoryCache(setupAction);
        }

        builder.Services.TryAddSingleton<IOllamaCache, OllamaMemoryCache>();
        return builder;
    }

    public static IOllamaClientBuilder WithDistributedCache(this IOllamaClientBuilder builder)
    {
        builder.Services.AddDistributedMemoryCache();
        builder.Services.TryAddSingleton<IOllamaCache, OllamaDistributedCache>();

        return builder;
    }

    public static IOllamaClientBuilder WithCache<TCacheImplementation>(this IOllamaClientBuilder builder) where TCacheImplementation : class, IOllamaCache
    {
        builder.Services.TryAddSingleton<IOllamaCache, TCacheImplementation>();
        return builder;
    }

    public static IHttpClientBuilder AddTransientHttpErrorPolicy(this IOllamaClientBuilder builder, Func<PolicyBuilder<HttpResponseMessage>, IAsyncPolicy<HttpResponseMessage>> configurePolicy)
        => builder.HttpClientBuilder.AddTransientHttpErrorPolicy(configurePolicy);
}
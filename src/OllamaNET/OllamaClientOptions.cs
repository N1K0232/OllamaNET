using OllamaNET.Models;

namespace OllamaNET;

public class OllamaClientOptions
{
    internal const string DefaultServiceUrl = "http://localhost:11434";

    public string ServiceUrl { get; set; } = DefaultServiceUrl;

    public string? ApiKey { get; set; }

    public string DefaultModel { get; set; } = OllamaChatModels.LLama32;

    public int MessageLimit { get; set; } = 10;

    public TimeSpan MessageExpiration { get; set; } = TimeSpan.FromHours(1);
}
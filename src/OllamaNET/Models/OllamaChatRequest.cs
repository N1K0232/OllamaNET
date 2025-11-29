using System.Text.Json.Serialization;

namespace OllamaNET.Models;

internal class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("messages")]
    public IList<OllamaChatMessage> Messages { get; set; } = [];

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    [JsonPropertyName("options")]
    public OllamaChatOptions? Options { get; set; }

    [JsonPropertyName("images")]
    public string[]? Images { get; set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; }

    [JsonPropertyName("raw")]
    public bool? Raw { get; set; }

    [JsonPropertyName("think")]
    public bool? Think { get; set; }
}
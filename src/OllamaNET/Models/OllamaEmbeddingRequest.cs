using System.Text.Json.Serialization;

namespace OllamaNET.Models;

internal class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = null!;
}
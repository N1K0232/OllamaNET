using System.Text.Json.Serialization;

namespace OllamaNET.Models;

public class OllamaStreamChunk
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("message")]
    public OllamaChatMessage? Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
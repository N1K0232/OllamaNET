using System.Text.Json.Serialization;

namespace OllamaNET.Models;

public class OllamaChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
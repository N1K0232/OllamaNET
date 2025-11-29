using System.Text.Json.Serialization;

namespace OllamaNET.Models;

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = null!;
}
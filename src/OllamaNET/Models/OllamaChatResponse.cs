using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OllamaNET.Models;

public class OllamaChatResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("message")]
    public OllamaChatMessage Message { get; set; } = null!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    public int EvalCount { get; set; }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OllamaNET.Models;

public class OllamaChatOptions
{
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; set; }

    [JsonPropertyName("repeat_penalty")]
    public float? RepeatPenalty { get; set; }

    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }

    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; set; }

    [JsonPropertyName("num_thread")]
    public int? NumThread { get; set; }

    [JsonPropertyName("mirostat")]
    public int? Mirostat { get; set; }

    [JsonPropertyName("mirostat_tau")]
    public float? MirostatTau { get; set; }

    [JsonPropertyName("mirostat_eta")]
    public float? MirostatEta { get; set; }
}
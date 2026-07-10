using System.Text.Json.Serialization;

namespace OpenRouterClient.DTO;

/// <summary>
/// Represents reasoning configuration for the request.
/// </summary>
public class ReasoningDto
{
    /// <summary>
    /// Optional. OpenAI-style reasoning effort setting (high, medium, low).
    /// </summary>
    [JsonPropertyName("effort")]
    public string Effort { get; set; }

    /// <summary>
    /// Optional. Non-OpenAI-style reasoning effort setting (max tokens).
    /// Cannot be used simultaneously with effort.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Optional. Whether to exclude reasoning from the response. Defaults to false.
    /// </summary>
    [JsonPropertyName("exclude")]
    public bool? Exclude { get; set; }
}
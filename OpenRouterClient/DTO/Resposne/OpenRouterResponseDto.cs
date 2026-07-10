namespace OpenRouterClient.DTO;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a response from the OpenRouter API for chat completions.
/// </summary>
public class OpenRouterResponseDto
{
    /// <summary>
    /// Optional. Unique identifier for the completion.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Optional. List of completion choices.
    /// </summary>
    [JsonPropertyName("choices")]
    public List<ChoiceDto> Choices { get; set; }
}

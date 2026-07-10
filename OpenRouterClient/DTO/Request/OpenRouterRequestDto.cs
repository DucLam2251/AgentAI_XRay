using OpenRouterClient.DTO.Request;

namespace OpenRouterClient.DTO;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a request to the OpenRouter API for generating completions or chat-based responses.
/// </summary>
public class OpenRouterRequestDto
{
    /// <summary>
    /// Required. The model ID to use. If unspecified, the user's default is used.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// Required. List of message objects describing the conversation.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; } = null;

    /// <summary>
    /// Optional. Enable streaming of results. Defaults to false.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Optional. Maximum number of tokens to generate (range: [1, context_length)).
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Optional. Sampling temperature (range: [0, 2]).
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Optional. Seed for deterministic outputs.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Optional. Top-p sampling value (range: (0, 1]).
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// Optional. Top-k sampling value (range: [1, Infinity)).
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// Optional. Frequency penalty (range: [-2, 2]).
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Optional. Presence penalty (range: [-2, 2]).
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Optional. Repetition penalty (range: (0, 2]).
    /// </summary>
    [JsonPropertyName("repetition_penalty")]
    public double? RepetitionPenalty { get; set; }

    /// <summary>
    /// Optional. Mapping of token IDs to bias values.
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, double>? LogitBias { get; set; } = null;

    /// <summary>
    /// Optional. Number of top log probabilities to return.
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

   
    
    /// <summary>
    /// Optional. Minimum probability threshold (range: [0, 1]).
    /// </summary>
    [JsonPropertyName("min_p")]
    public double? MinP { get; set; }

    /// <summary>
    /// Optional. Alternate top sampling parameter (range: [0, 1]).
    /// </summary>
    [JsonPropertyName("top_a")]
    public double? TopA { get; set; }

    /// <summary>
    /// Optional. List of prompt transforms (OpenRouter-only).
    /// </summary>
    [JsonPropertyName("transforms")]
    public List<string>? Transforms { get; set; } = null;

    /// <summary>
    /// Optional. Alternate list of models for routing overrides.
    /// </summary>
    [JsonPropertyName("models")]
    public List<string>? Models { get; set; } = null;

    // /// <summary>
    // /// Optional. Preferences for provider routing.
    // /// </summary>
    // [JsonPropertyName("provider")]
    // public Dictionary<string, object> Provider { get; set; } = new();

    /// <summary>
    /// Optional. Sort preference (e.g., price, throughput).
    /// </summary>
    [JsonPropertyName("sort")]
    public string Sort { get; set; }

    /// <summary>
    /// Optional. Configuration for model reasoning/thinking tokens.
    /// </summary>
    [JsonPropertyName("reasoning")]
    public ReasoningDto Reasoning { get; set; }

    /// <summary>
    /// Optional. A unique identifier representing your end-user.
    /// </summary>
    [JsonPropertyName("user")]
    public string User { get; set; }
}


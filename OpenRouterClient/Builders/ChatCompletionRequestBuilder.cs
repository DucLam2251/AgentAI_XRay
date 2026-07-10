using OpenRouterClient.DTO;
using OpenRouterClient.Services;
using System.Collections.Generic;
using System.Linq;
using OpenRouterClient.DTO.Request;

namespace OpenRouterClient.Builders;

/// <summary>
/// A fluent builder for constructing and sending OpenRouter chat completion requests.
/// Provides methods to configure all parameters supported by the OpenRouter API.
/// </summary>
public class ChatCompletionRequestBuilder
{
    private readonly OpenRouterRequestDto _request = new();
    private readonly ServiceOrchestrator _serviceOrchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatCompletionRequestBuilder"/> class.
    /// </summary>
    /// <param name="serviceOrchestrator">The service orchestrator instance used for sending requests.</param>
    public ChatCompletionRequestBuilder(ServiceOrchestrator serviceOrchestrator)
    {
        _serviceOrchestrator = serviceOrchestrator;
        _request.Messages = new List<MessageDto>();
    }

    /// <summary>
    /// Specifies the model to use for the completion.
    /// </summary>
    /// <param name="model">The model ID (e.g., "openrouter/auto" or specific model name).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithModel(string model)
    {
        _request.Model = model;
        return this;
    }

    /// <summary>
    /// Adds a message to the conversation history.
    /// </summary>
    /// <param name="role">The role of the message sender ("system", "user", or "assistant").</param>
    /// <param name="content">The content of the message.</param>
    /// <returns>The builder instance for method chaining.</returns>
    private ChatCompletionRequestBuilder AddMessage(string role, string content)
    {
        _request.Messages.Add(new MessageDto { Role = role, Content = content });
        return this;
    }

    /// <summary>
    /// Adds a user message to the conversation history.
    /// </summary>
    /// <param name="content">The content of the user message.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder AddUserMessage(string content) => AddMessage("user", content);

    /// <summary>
    /// Adds an assistant message to the conversation history.
    /// </summary>
    /// <param name="content">The content of the assistant message.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder AddAssistantMessage(string content) => AddMessage("assistant", content);

    /// <summary>
    /// Adds a system message to the conversation history.
    /// </summary>
    /// <param name="content">The content of the system message.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder AddSystemMessage(string content) => AddMessage("system", content);

    /// <summary>
    /// Sets the maximum number of tokens to generate in the completion.
    /// </summary>
    /// <param name="maxTokens">Maximum tokens (range: [1, context_length]).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithMaxTokens(int maxTokens)
    {
        _request.MaxTokens = maxTokens;
        return this;
    }

    /// <summary>
    /// Sets the sampling temperature for more creative/random outputs.
    /// </summary>
    /// <param name="temperature">Value between 0 and 2. Higher values = more random.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithTemperature(double temperature)
    {
        _request.Temperature = temperature;
        return this;
    }

    /// <summary>
    /// Sets the random seed for deterministic outputs.
    /// </summary>
    /// <param name="seed">The seed value for random generation.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithSeed(int seed)
    {
        _request.Seed = seed;
        return this;
    }

    /// <summary>
    /// Sets top-p (nucleus) sampling parameter.
    /// </summary>
    /// <param name="topP">Value between 0 and 1. Lower values = more focused outputs.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithTopP(double topP)
    {
        _request.TopP = topP;
        return this;
    }

    /// <summary>
    /// Sets top-k sampling parameter.
    /// </summary>
    /// <param name="topK">Number of highest probability tokens to consider (range: [1, ∞)).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithTopK(int topK)
    {
        _request.TopK = topK;
        return this;
    }

    /// <summary>
    /// Sets minimum probability threshold for token selection.
    /// </summary>
    /// <param name="minP">Value between 0 and 1.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithMinP(double minP)
    {
        _request.MinP = minP;
        return this;
    }

    /// <summary>
    /// Sets top-a sampling parameter (alternative to top-p/top-k).
    /// </summary>
    /// <param name="topA">Value between 0 and 1.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithTopA(double topA)
    {
        _request.TopA = topA;
        return this;
    }

    /// <summary>
    /// Sets frequency penalty to reduce repetition of tokens.
    /// </summary>
    /// <param name="frequencyPenalty">Value between -2 and 2.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithFrequencyPenalty(double frequencyPenalty)
    {
        _request.FrequencyPenalty = frequencyPenalty;
        return this;
    }

    /// <summary>
    /// Sets presence penalty to encourage new topics.
    /// </summary>
    /// <param name="presencePenalty">Value between -2 and 2.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithPresencePenalty(double presencePenalty)
    {
        _request.PresencePenalty = presencePenalty;
        return this;
    }

    /// <summary>
    /// Sets repetition penalty to control token repetition.
    /// </summary>
    /// <param name="repetitionPenalty">Value between 0 and 2.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithRepetitionPenalty(double repetitionPenalty)
    {
        _request.RepetitionPenalty = repetitionPenalty;
        return this;
    }

    /// <summary>
    /// Sets logit bias for specific tokens.
    /// </summary>
    /// <param name="logitBias">Dictionary mapping token IDs to bias values.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithLogitBias(Dictionary<string, double>? logitBias)
    {
        _request.LogitBias = logitBias;
        return this;
    }

    /// <summary>
    /// Sets number of top log probabilities to return.
    /// </summary>
    /// <param name="topLogprobs">Number of logprobs to return.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithTopLogprobs(int topLogprobs)
    {
        _request.TopLogprobs = topLogprobs;
        return this;
    }

    /// <summary>
    /// Enables or disables streaming of partial results.
    /// </summary>
    /// <param name="stream">True to enable streaming (default: true).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithStreaming(bool stream = true)
    {
        _request.Stream = stream;
        return this;
    }
    

    /// <summary>
    /// Sets prompt transforms (OpenRouter-specific).
    /// </summary>
    /// <param name="transforms">List of transform names.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithTransforms(params string[] transforms)
    {
        _request.Transforms = transforms.ToList();
        return this;
    }

    /// <summary>
    /// Sets alternative models for routing overrides.
    /// </summary>
    /// <param name="models">List of alternative model IDs.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithModelRouting(params string[] models)
    {
        _request.Models = models.ToList();
        return this;
    }
    //
    // /// <summary>
    // /// Sets provider-specific settings.
    // /// </summary>
    // /// <param name="providerSettings">Dictionary of provider settings.</param>
    // /// <returns>The builder instance for method chaining.</returns>
    // public ChatCompletionRequestBuilder WithProviderSettings(Dictionary<string, object> providerSettings)
    // {
    //     _request.Provider = providerSettings;
    //     return this;
    // }

    /// <summary>
    /// Sets model sorting preference.
    /// </summary>
    /// <param name="sort">Sort preference (e.g., "price", "throughput").</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithSortPreference(string sort)
    {
        _request.Sort = sort;
        return this;
    }

    /// <summary>
    /// Configures reasoning/thinking tokens.
    /// </summary>
    /// <param name="effort">Effort level ("high", "medium", "low").</param>
    /// <param name="maxTokens">Max tokens for reasoning.</param>
    /// <param name="exclude">Whether to exclude reasoning from response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithReasoning(string effort, int? maxTokens = null, bool exclude = false)
    {
        _request.Reasoning = new ReasoningDto
        {
            Effort = effort,
            MaxTokens = maxTokens,
            Exclude = exclude
        };
        return this;
    }

    /// <summary>
    /// Sets user identifier for monitoring.
    /// </summary>
    /// <param name="user">Unique user identifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ChatCompletionRequestBuilder WithUser(string user)
    {
        _request.User = user;
        return this;
    }

    /// <summary>
    /// Sends the constructed request to OpenRouter and returns the response.
    /// </summary>
    /// <returns>The completion response or null if failed.</returns>
    public async Task<OpenRouterResponseDto?> SendAsync()
    {
        return await _serviceOrchestrator.ChatCompletionService.CreateChatAsync(_request);
    }

    /// <summary>
    /// Returns the configured request object without sending it.
    /// </summary>
    /// <returns>The fully configured OpenRouterRequestDto.</returns>
    public OpenRouterRequestDto Build()
    {
        return _request;
    }
}
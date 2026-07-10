using OpenRouterClient.Builders;
using OpenRouterClient.Services;
using OpenRouterClient.Utilities;

/// <summary>
/// Represents the main client for interacting with the OpenRouter API.
/// Initializes global configuration and provides access to chat completion requests.
/// </summary>
public class Client
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class with the specified API URL and token.
    /// </summary>
    /// <param name="apiUrl">The base URL of the OpenRouter API.</param>
    /// <param name="apiToken">The API token used for authentication.</param>
    public Client(string apiUrl, string apiToken)
    {
        OpenRouterClientGlobalConfig.ApiUrl = apiUrl;
        OpenRouterClientGlobalConfig.ApiToken = apiToken;

        Chat = new ChatCompletionRequestBuilder(new ServiceOrchestrator());
    }

    /// <summary>
    /// Gets or sets the chat completion request builder.
    /// </summary>
    public ChatCompletionRequestBuilder Chat { get; set; }
}
namespace OpenRouterClient.Services;

public class ServiceOrchestrator {
    
    /// <summary>
    /// Service responsible for sending chat completion requests to the OpenRouter API.
    /// </summary>
    public ChatCompletionService ChatCompletionService { get; } = new ChatCompletionService();
}

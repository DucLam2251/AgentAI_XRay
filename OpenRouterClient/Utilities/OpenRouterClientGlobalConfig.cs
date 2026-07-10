namespace OpenRouterClient.Utilities;

public static class OpenRouterClientGlobalConfig {
    public static string ApiUrl { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
    public static string ApiToken { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
}
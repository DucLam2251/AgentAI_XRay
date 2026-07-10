using System.Text.Json.Serialization;



namespace OpenRouterClient.DTO.Request;

/// <summary>
/// Represents a message in the OpenRouter conversation.
/// </summary>
public class MessageDto
{
    /// <summary>
    /// The role of the message sender. Must be 'system', 'user', or 'assistant'.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// The content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    public override string ToString() {
        return Content;
    }
}

using OpenRouterClient.DTO;

namespace OpenRouterClient.Services;

/// <summary>
/// Service responsible for sending chat completion requests to the OpenRouter API.
/// </summary>
public class ChatCompletionService
{
    /// <summary>
    /// Sends a chat completion request to the OpenRouter API and returns the response.
    /// </summary>
    /// <param name="OpenRouterRequestDto"> <see cref="OpenRouterResponseDto"/> The request object containing all parameters for the OpenRouter model.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="OpenRouterResponseDto"/>
    /// which includes the generated model response and related metadata.
    /// </returns>
    public async Task<OpenRouterResponseDto?> CreateChatAsync(OpenRouterRequestDto chatCompletionRequestDto)
    {
        return await HttpRequestService._PostRequestAsync<OpenRouterRequestDto, OpenRouterResponseDto>(
            chatCompletionRequestDto);
    }
}
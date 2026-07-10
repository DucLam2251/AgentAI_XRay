using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenRouterClient.Utilities;

namespace OpenRouterClient.Services;

public static class HttpRequestService {
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Sends an asynchronous HTTP POST request with a JSON body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request DTO.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response DTO.</typeparam>
    /// <param name="requestDto">The request data transfer object containing the request payload.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the deserialized response,
    /// or null if an error occurred.
    /// </returns>
    public static async Task<TResponse?> _PostRequestAsync<TRequest, TResponse>(TRequest requestDto)
        where TRequest : class
        where TResponse : class {
        try {
            var requestJson = JsonSerializer.Serialize(requestDto, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                IgnoreReadOnlyProperties = true
            });
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, OpenRouterClientGlobalConfig.ApiUrl) {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OpenRouterClientGlobalConfig.ApiToken);

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode) {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error ({response.StatusCode}): {error}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(content);
            var parsed = JsonSerializer.Deserialize<TResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return parsed;
        }
        catch (Exception e) {
            Console.WriteLine($"Exception: {e.Message}");
            return null;
        }
    }


    /// <summary>
    /// Sends an asynchronous HTTP Get request with a JSON body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request DTO.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response DTO.</typeparam>
    /// <param name="requestDto">The request data transfer object containing the request payload.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the deserialized response,
    /// or null if an error occurred.
    /// </returns>
    public static async Task<TResponse?> _GetRequestAsync<TRequest, TResponse>(TRequest requestDto)
        where TRequest : class
        where TResponse : class {
        try {
            var requestJson = JsonSerializer.Serialize(requestDto);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, OpenRouterClientGlobalConfig.ApiUrl) {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OpenRouterClientGlobalConfig.ApiToken);

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode) {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error ({response.StatusCode}): {error}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<TResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return parsed;
        }
        catch (Exception e) {
            Console.WriteLine($"Exception: {e.Message}");
            return null;
        }
    }
}
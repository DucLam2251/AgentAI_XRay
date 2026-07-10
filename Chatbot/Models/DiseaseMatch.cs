using System.Text.Json.Serialization;

namespace Chatbot.Models
{
    public class DiseaseMatch
    {
        [JsonPropertyName("disease")]
        public Disease Disease { get; set; } = new Disease();

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }
}

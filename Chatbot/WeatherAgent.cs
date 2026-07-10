using System.Text;
using System.Text.Json;

namespace Chatbot
{
    public class WeatherAgent
    {
        private readonly Handle _handle;

        public WeatherAgent(string apiKey)
        {
            _handle = new Handle(apiKey);
        }

        public DiseaseStore GetStore() => _handle.GetStore();

        public async Task<AgentResponse> AskAsync(string userInput)
        {
            return await _handle.AskAsync(userInput);
        }
    }
}
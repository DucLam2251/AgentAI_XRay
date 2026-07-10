using System;
using System.Collections.Generic;
using System.Text;

namespace Chatbot
{
    public class AgentResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public string? ToolName { get; set; }

        public object? Data { get; set; }

        public string? Error { get; set; }
    }
    public class RequestBodyToAPI
    {
        public string model { get; set; } = "openai/gpt-4o-mini";

        public object messages { get; set; } 
        public object tools { get; set; }
    }
}

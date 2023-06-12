﻿using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel
{
    public class ChatCompletionMessage
    {
        public ChatCompletionMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}


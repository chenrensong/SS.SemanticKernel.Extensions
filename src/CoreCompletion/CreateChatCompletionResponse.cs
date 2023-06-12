using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel
{
    public record CreateChatCompletionResponse : ApiResponseBase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("object")]
        public string Object { get; set; } = null!;

        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public ChatCompletionUsage? Usage { get; set; }
    }
}


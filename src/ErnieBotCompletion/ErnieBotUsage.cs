using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel {
    public class ErnieBotUsage
    {
        [JsonProperty("prompt_tokens")]
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}

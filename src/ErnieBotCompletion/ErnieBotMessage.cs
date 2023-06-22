using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel {
    public class ErnieBotMessage
    {

        public ErnieBotMessage(string role, string content)
        {
            Role = role.ToLower();
            Content = content;
        }


        [JsonProperty("role")]
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}

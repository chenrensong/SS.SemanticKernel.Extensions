using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel {
    public class ErnieBotCompletionResponse
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("error_code")]
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_msg")]
        [JsonPropertyName("error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("object")]
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonProperty("result")]
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonProperty("need_clear_history")]
        [JsonPropertyName("need_clear_history")]
        public bool NeedClearHistory { get; set; }

        [JsonProperty("usage")]
        [JsonPropertyName("usage")]
        public ErnieBotUsage Usage { get; set; }
    }
}

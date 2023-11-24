using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel
{
    public abstract record ApiResponseBase
    {
        [JsonPropertyName("error")]
        public ApiError? Error { get; set; }
    }
}


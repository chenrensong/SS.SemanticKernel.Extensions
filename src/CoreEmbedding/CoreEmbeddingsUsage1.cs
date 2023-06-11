using System.Text.Json;

namespace Microsoft.SemanticKernel.AI.Embeddings {
    public partial class CoreEmbeddingsUsage {
        internal static CoreEmbeddingsUsage DeserializeEmbeddingsUsage(JsonElement element) {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
            int promptTokens = default;
            int totalTokens = default;
            foreach (var property in element.EnumerateObject()) {
                if (property.NameEquals("prompt_tokens"u8)) {
                    promptTokens = property.Value.GetInt32();
                    continue;
                }
                if (property.NameEquals("total_tokens"u8)) {
                    totalTokens = property.Value.GetInt32();
                    continue;
                }
            }
            return new CoreEmbeddingsUsage(promptTokens, totalTokens);
        }

        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static CoreEmbeddingsUsage FromResponse(string response) {
            using var document = JsonDocument.Parse(response);
            return DeserializeEmbeddingsUsage(document.RootElement);
        }
    }
}

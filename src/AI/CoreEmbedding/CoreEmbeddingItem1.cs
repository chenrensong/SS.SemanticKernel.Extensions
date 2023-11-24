using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.SemanticKernel.AI.Embeddings {
    public partial class CoreEmbeddingItem {
        internal static CoreEmbeddingItem DeserializeEmbeddingItem(JsonElement element) {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
            IReadOnlyList<float> embedding = default;
            int index = default;
            foreach (var property in element.EnumerateObject()) {
                if (property.NameEquals("embedding"u8)) {
                    List<float> array = new List<float>();
                    foreach (var item in property.Value.EnumerateArray()) {
                        array.Add(item.GetSingle());
                    }
                    embedding = array;
                    continue;
                }
                if (property.NameEquals("index"u8)) {
                    index = property.Value.GetInt32();
                    continue;
                }
            }
            return new CoreEmbeddingItem(embedding, index);
        }

        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static CoreEmbeddingItem FromResponse(string response) {
            using var document = JsonDocument.Parse(response);
            return DeserializeEmbeddingItem(document.RootElement);
        }
    }
}

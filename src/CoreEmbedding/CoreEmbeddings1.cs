using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.SemanticKernel.AI.Embeddings {
    public partial class CoreEmbeddings {

        internal static CoreEmbeddings DeserializeEmbeddings(JsonElement element) {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
            IReadOnlyList<CoreEmbeddingItem> data = default;
            CoreEmbeddingsUsage usage = default;
            foreach (var property in element.EnumerateObject()) {
                if (property.NameEquals("data"u8)) {
                    List<CoreEmbeddingItem> array = new List<CoreEmbeddingItem>();
                    foreach (var item in property.Value.EnumerateArray()) {
                        array.Add(CoreEmbeddingItem.DeserializeEmbeddingItem(item));
                    }
                    data = array;
                    continue;
                }
                if (property.NameEquals("usage"u8)) {
                    usage = CoreEmbeddingsUsage.DeserializeEmbeddingsUsage(property.Value);
                    continue;
                }
            }
            return new CoreEmbeddings(data, usage);
        }

        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static CoreEmbeddings FromResponse(string response) {
            using var document = JsonDocument.Parse(response);
            return DeserializeEmbeddings(document.RootElement);
        }
    }
}

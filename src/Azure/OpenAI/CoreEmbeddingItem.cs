using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreEmbeddingItem
    {
        public IReadOnlyList<float> Embedding { get; }

        public int Index { get; }

        internal CoreEmbeddingItem(IEnumerable<float> embedding, int index)
        {
            Azure.Core.Argument.AssertNotNull(embedding, "embedding");
            Embedding = embedding.ToList();
            Index = index;
        }

        internal CoreEmbeddingItem(IReadOnlyList<float> embedding, int index)
        {
            Embedding = embedding;
            Index = index;
        }

        internal static CoreEmbeddingItem DeserializeEmbeddingItem(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            IReadOnlyList<float> embedding = null;
            int index = 0;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[9] { 101, 109, 98, 101, 100, 100, 105, 110, 103 }))
                {
                    List<float> list = new List<float>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(item2.GetSingle());
                    }
                    embedding = list;
                }
                else if (item.NameEquals(new byte[5] { 105, 110, 100, 101, 120 }))
                {
                    index = item.Value.GetInt32();
                }
            }
            return new CoreEmbeddingItem(embedding, index);
        }

        internal static CoreEmbeddingItem FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeEmbeddingItem(jsonDocument.RootElement);
            }
        }
    }

}

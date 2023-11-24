using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreEmbeddingsUsage
    {
        public int PromptTokens { get; }

        public int TotalTokens { get; }

        internal CoreEmbeddingsUsage(int promptTokens, int totalTokens)
        {
            PromptTokens = promptTokens;
            TotalTokens = totalTokens;
        }

        internal static CoreEmbeddingsUsage DeserializeEmbeddingsUsage(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            int promptTokens = 0;
            int totalTokens = 0;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[13]
                {
                112, 114, 111, 109, 112, 116, 95, 116, 111, 107,
                101, 110, 115
                }))
                {
                    promptTokens = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[12]
                {
                116, 111, 116, 97, 108, 95, 116, 111, 107, 101,
                110, 115
                }))
                {
                    totalTokens = item.Value.GetInt32();
                }
            }
            return new CoreEmbeddingsUsage(promptTokens, totalTokens);
        }

        internal static CoreEmbeddingsUsage FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeEmbeddingsUsage(jsonDocument.RootElement);
            }
        }
    }

}

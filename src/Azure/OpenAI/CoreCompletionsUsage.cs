using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreCompletionsUsage
    {
        public int CompletionTokens { get; }

        public int PromptTokens { get; }

        public int TotalTokens { get; }

        internal CoreCompletionsUsage(int completionTokens, int promptTokens, int totalTokens)
        {
            CompletionTokens = completionTokens;
            PromptTokens = promptTokens;
            TotalTokens = totalTokens;
        }

        internal static CoreCompletionsUsage DeserializeCompletionsUsage(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            int completionTokens = 0;
            int promptTokens = 0;
            int totalTokens = 0;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[17]
                {
                99, 111, 109, 112, 108, 101, 116, 105, 111, 110,
                95, 116, 111, 107, 101, 110, 115
                }))
                {
                    completionTokens = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[13]
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
            return new CoreCompletionsUsage(completionTokens, promptTokens, totalTokens);
        }

        internal static CoreCompletionsUsage FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeCompletionsUsage(jsonDocument.RootElement);
            }
        }
    }





}
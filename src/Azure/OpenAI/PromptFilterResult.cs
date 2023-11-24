using Azure.AI.OpenAI;
using Azure.Core;
using Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class PromptFilterResult
    {
        public int PromptIndex { get; }

        public CoreContentFilterResults ContentFilterResults { get; }

        internal PromptFilterResult(int promptIndex)
        {
            PromptIndex = promptIndex;
        }

        internal PromptFilterResult(int promptIndex, CoreContentFilterResults contentFilterResults)
        {
            PromptIndex = promptIndex;
            ContentFilterResults = contentFilterResults;
        }

        internal static PromptFilterResult DeserializePromptFilterResult(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            int promptIndex = 0;
            Optional<CoreContentFilterResults> optional = default(Optional<CoreContentFilterResults>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[12]
                {
                112, 114, 111, 109, 112, 116, 95, 105, 110, 100,
                101, 120
                }))
                {
                    promptIndex = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[22]
                {
                99, 111, 110, 116, 101, 110, 116, 95, 102, 105,
                108, 116, 101, 114, 95, 114, 101, 115, 117, 108,
                116, 115
                }) && item.Value.ValueKind != JsonValueKind.Null)
                {
                    optional = CoreContentFilterResults.DeserializeContentFilterResults(item.Value);
                }
            }
            return new PromptFilterResult(promptIndex, optional.Value);
        }

        internal static PromptFilterResult FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializePromptFilterResult(jsonDocument.RootElement);
            }
        }
    }

}

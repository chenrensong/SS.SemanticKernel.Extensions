using Azure.Core;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreChatChoice
    {
        internal CoreChatMessage InternalStreamingDeltaMessage { get; }

        public CoreChatMessage Message { get; }

        public int Index { get; }

        public CompletionsFinishReason? FinishReason { get; }

        public CoreContentFilterResults ContentFilterResults { get; }

        internal CoreChatChoice(int index, CompletionsFinishReason? finishReason)
        {
            Index = index;
            FinishReason = finishReason;
        }

        internal CoreChatChoice(CoreChatMessage message, int index, CompletionsFinishReason? finishReason, CoreChatMessage internalStreamingDeltaMessage, CoreContentFilterResults contentFilterResults)
        {
            Message = message;
            Index = index;
            FinishReason = finishReason;
            InternalStreamingDeltaMessage = internalStreamingDeltaMessage;
            ContentFilterResults = contentFilterResults;
        }

        internal static CoreChatChoice DeserializeChatChoice(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            Optional<CoreChatMessage> optional = default(Optional<CoreChatMessage>);
            int index = 0;
            CompletionsFinishReason? finishReason = null;
            Optional<CoreChatMessage> optional2 = default(Optional<CoreChatMessage>);
            Optional<CoreContentFilterResults> optional3 = default(Optional<CoreContentFilterResults>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[7] { 109, 101, 115, 115, 97, 103, 101 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional = CoreChatMessage.DeserializeChatMessage(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[5] { 105, 110, 100, 101, 120 }))
                {
                    index = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[13]
                {
                102, 105, 110, 105, 115, 104, 95, 114, 101, 97,
                115, 111, 110
                }))
                {
                    finishReason = ((item.Value.ValueKind != JsonValueKind.Null) ? new CompletionsFinishReason?(new CompletionsFinishReason(item.Value.GetString())) : null);
                }
                else if (item.NameEquals(new byte[5] { 100, 101, 108, 116, 97 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional2 = CoreChatMessage.DeserializeChatMessage(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[22]
                {
                99, 111, 110, 116, 101, 110, 116, 95, 102, 105,
                108, 116, 101, 114, 95, 114, 101, 115, 117, 108,
                116, 115
                }) && item.Value.ValueKind != JsonValueKind.Null)
                {
                    optional3 = CoreContentFilterResults.DeserializeContentFilterResults(item.Value);
                }
            }
            return new CoreChatChoice(optional.Value, index, finishReason, optional2.Value, optional3.Value);
        }

        internal static CoreChatChoice FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeChatChoice(jsonDocument.RootElement);
            }
        }
    }


}

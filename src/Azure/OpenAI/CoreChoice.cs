using Azure.Core;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreChoice
    {
        public string Text { get; }

        public int Index { get; }

        public CoreContentFilterResults ContentFilterResults { get; }

        public CoreCompletionsLogProbabilityModel LogProbabilityModel { get; }

        public CompletionsFinishReason? FinishReason { get; }

        internal CoreChoice(string text, int index, CoreCompletionsLogProbabilityModel logProbabilityModel, CompletionsFinishReason finishReason)
        {
            Azure.Core.Argument.AssertNotNull(text, "text");
            Text = text;
            Index = index;
            LogProbabilityModel = logProbabilityModel;
            FinishReason = finishReason;
        }

        internal CoreChoice(string text, int index, CoreCompletionsLogProbabilityModel logProbabilityModel, CompletionsFinishReason? finishReason)
        {
            Azure.Core.Argument.AssertNotNull(text, "text");
            Text = text;
            Index = index;
            LogProbabilityModel = logProbabilityModel;
            FinishReason = finishReason;
        }

        internal CoreChoice(string text, int index, CoreContentFilterResults contentFilterResults, CoreCompletionsLogProbabilityModel logProbabilityModel, CompletionsFinishReason? finishReason)
        {
            Text = text;
            Index = index;
            ContentFilterResults = contentFilterResults;
            LogProbabilityModel = logProbabilityModel;
            FinishReason = finishReason;
        }

        internal static CoreChoice DeserializeChoice(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string text = null;
            int index = 0;
            Optional<CoreContentFilterResults> optional = default(Optional<CoreContentFilterResults>);
            CoreCompletionsLogProbabilityModel logProbabilityModel = null;
            CompletionsFinishReason? finishReason = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[4] { 116, 101, 120, 116 }))
                {
                    text = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[5] { 105, 110, 100, 101, 120 }))
                {
                    index = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[22]
                {
                99, 111, 110, 116, 101, 110, 116, 95, 102, 105,
                108, 116, 101, 114, 95, 114, 101, 115, 117, 108,
                116, 115
                }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional = CoreContentFilterResults.DeserializeContentFilterResults(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[8] { 108, 111, 103, 112, 114, 111, 98, 115 }))
                {
                    logProbabilityModel = ((item.Value.ValueKind != JsonValueKind.Null) ? CoreCompletionsLogProbabilityModel.DeserializeCompletionsLogProbabilityModel(item.Value) : null);
                }
                else if (item.NameEquals(new byte[13]
                {
                102, 105, 110, 105, 115, 104, 95, 114, 101, 97,
                115, 111, 110
                }))
                {
                    finishReason = ((item.Value.ValueKind != JsonValueKind.Null) ? new CompletionsFinishReason?(new CompletionsFinishReason(item.Value.GetString())) : null);
                }
            }
            return new CoreChoice(text, index, optional.Value, logProbabilityModel, finishReason);
        }

        internal static CoreChoice FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeChoice(jsonDocument.RootElement);
            }
        }
    }





}
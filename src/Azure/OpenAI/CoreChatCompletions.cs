using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreChatCompletions
    {
        public string Id { get; }

        public DateTimeOffset Created { get; }

        public IReadOnlyList<CoreChatChoice> Choices { get; }

        public IReadOnlyList<PromptFilterResult> PromptFilterResults { get; }

        public CoreCompletionsUsage Usage { get; }

        internal static CoreChatCompletions DeserializeChatCompletions(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string id = null;
            DateTimeOffset created = default(DateTimeOffset);
            IReadOnlyList<CoreChatChoice> choices = null;
            Optional<IReadOnlyList<PromptFilterResult>> optional = default(Optional<IReadOnlyList<PromptFilterResult>>);
            CoreCompletionsUsage usage = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[2] { 105, 100 }))
                {
                    id = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[7] { 99, 114, 101, 97, 116, 101, 100 }))
                {
                    created = DateTimeOffset.FromUnixTimeSeconds(item.Value.GetInt64());
                }
                else if (item.NameEquals(new byte[7] { 99, 104, 111, 105, 99, 101, 115 }))
                {
                    List<CoreChatChoice> list = new List<CoreChatChoice>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(CoreChatChoice.DeserializeChatChoice(item2));
                    }
                    choices = list;
                }
                else if (item.NameEquals(new byte[18]
                {
                112, 114, 111, 109, 112, 116, 95, 97, 110, 110,
                111, 116, 97, 116, 105, 111, 110, 115
                }) || item.NameEquals(new byte[21]
                {
                112, 114, 111, 109, 112, 116, 95, 102, 105, 108,
                116, 101, 114, 95, 114, 101, 115, 117, 108, 116,
                115
                }))
                {
                    if (item.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<PromptFilterResult> list2 = new List<PromptFilterResult>();
                    foreach (JsonElement item3 in item.Value.EnumerateArray())
                    {
                        list2.Add(PromptFilterResult.DeserializePromptFilterResult(item3));
                    }
                    optional = list2;
                }
                else if (item.NameEquals(new byte[5] { 117, 115, 97, 103, 101 }))
                {
                    usage = CoreCompletionsUsage.DeserializeCompletionsUsage(item.Value);
                }
            }
            return new CoreChatCompletions(id, created, choices, Optional.ToList(optional), usage);
        }

        internal CoreChatCompletions(string id, DateTimeOffset created, IEnumerable<CoreChatChoice> choices, CoreCompletionsUsage usage)
        {
            Azure.Core.Argument.AssertNotNull(id, "id");
            Azure.Core.Argument.AssertNotNull(choices, "choices");
            Azure.Core.Argument.AssertNotNull(usage, "usage");
            Id = id;
            Created = created;
            Choices = choices.ToList();
            PromptFilterResults = new ChangeTrackingList<PromptFilterResult>();
            Usage = usage;
        }

        internal CoreChatCompletions(string id, DateTimeOffset created, IReadOnlyList<CoreChatChoice> choices, IReadOnlyList<PromptFilterResult> promptFilterResults, CoreCompletionsUsage usage)
        {
            Id = id;
            Created = created;
            Choices = choices;
            PromptFilterResults = promptFilterResults;
            Usage = usage;
        }

        internal static CoreChatCompletions FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeChatCompletions(jsonDocument.RootElement);
            }
        }
    }


}

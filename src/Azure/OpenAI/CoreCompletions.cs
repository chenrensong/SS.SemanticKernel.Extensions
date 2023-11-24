using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreCompletions
    {
        public string Id { get; }

        public DateTimeOffset Created { get; }

        public IReadOnlyList<PromptFilterResult> PromptFilterResults { get; }

        public IReadOnlyList<CoreChoice> Choices { get; }

        public CoreCompletionsUsage Usage { get; }

        internal static CoreCompletions DeserializeCompletions(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string id = default;
            DateTimeOffset created = default;
            Optional<IReadOnlyList<PromptFilterResult>> promptAnnotations = default;
            IReadOnlyList<CoreChoice> choices = default;
            CoreCompletionsUsage usage = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("id"u8))
                {
                    id = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("created"u8))
                {
                    created = DateTimeOffset.FromUnixTimeSeconds(property.Value.GetInt64());
                    continue;
                }
                // CUSTOM CODE NOTE: temporary, custom handling of forked keys for prompt filter results
                if (property.NameEquals("prompt_annotations"u8) || property.NameEquals("prompt_filter_results"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<PromptFilterResult> array = new List<PromptFilterResult>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        array.Add(PromptFilterResult.DeserializePromptFilterResult(item));
                    }
                    promptAnnotations = array;
                    continue;
                }
                if (property.NameEquals("choices"u8))
                {
                    List<CoreChoice> array = new List<CoreChoice>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        array.Add(CoreChoice.DeserializeChoice(item));
                    }
                    choices = array;
                    continue;
                }
                if (property.NameEquals("usage"u8))
                {
                    usage = CoreCompletionsUsage.DeserializeCompletionsUsage(property.Value);
                    continue;
                }
            }
            return new CoreCompletions(id, created, Optional.ToList(promptAnnotations), choices, usage);
        }

        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static CoreCompletions FromResponse(Response response)
        {
            using var document = JsonDocument.Parse(response.Content);
            return DeserializeCompletions(document.RootElement);
        }

        internal CoreCompletions(string id, DateTimeOffset created, IEnumerable<CoreChoice> choices, CoreCompletionsUsage usage)
        {
            Azure.Core.Argument.AssertNotNull(id, "id");
            Azure.Core.Argument.AssertNotNull(choices, "choices");
            Azure.Core.Argument.AssertNotNull(usage, "usage");
            Id = id;
            Created = created;
            PromptFilterResults = new ChangeTrackingList<PromptFilterResult>();
            Choices = choices.ToList();
            Usage = usage;
        }

        internal CoreCompletions(string id, DateTimeOffset created, IReadOnlyList<PromptFilterResult> promptFilterResults, IReadOnlyList<CoreChoice> choices, CoreCompletionsUsage usage)
        {
            Id = id;
            Created = created;
            PromptFilterResults = promptFilterResults;
            Choices = choices;
            Usage = usage;
        }
    }





}
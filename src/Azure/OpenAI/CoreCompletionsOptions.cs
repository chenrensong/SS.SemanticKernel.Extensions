using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreChatCompletionsOptions : IUtf8JsonSerializable
    {
        public int? ChoiceCount { get; set; }

        public float? FrequencyPenalty { get; set; }

        public int? MaxTokens { get; set; }

        public float? NucleusSamplingFactor { get; set; }

        public float? PresencePenalty { get; set; }

        public IList<string> StopSequences { get; }

        public float? Temperature { get; set; }

        public IDictionary<int, int> TokenSelectionBiases { get; }

        public string User { get; set; }

        public IList<CoreFunctionDefinition> Functions { get; set; }

        public CoreFunctionDefinition FunctionCall { get; set; }

        public AzureChatExtensionsOptions AzureExtensionsOptions { get; set; }

        internal IList<AzureChatExtensionConfiguration> InternalAzureExtensionsDataSources { get; set; }

        internal string InternalNonAzureModelName { get; set; }

        internal bool? InternalShouldStreamResponse { get; set; }

        internal IDictionary<string, int> InternalStringKeyedTokenSelectionBiases { get; }

        public IList<CoreChatMessage> Messages { get; }

        public CoreChatCompletionsOptions(IEnumerable<CoreChatMessage> messages)
            : this()
        {
            Azure.Core.Argument.AssertNotNull(messages, "messages");
            foreach (CoreChatMessage message in messages)
            {
                Messages.Add(message);
            }
        }

        public CoreChatCompletionsOptions()
        {
            Messages = new ChangeTrackingList<CoreChatMessage>();
            TokenSelectionBiases = new ChangeTrackingDictionary<int, int>();
            InternalStringKeyedTokenSelectionBiases = new ChangeTrackingDictionary<string, int>();
            StopSequences = new ChangeTrackingList<string>();
            Functions = new ChangeTrackingList<CoreFunctionDefinition>();
            InternalAzureExtensionsDataSources = new ChangeTrackingList<AzureChatExtensionConfiguration>();
            AzureExtensionsOptions = null;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(new byte[8] { 109, 101, 115, 115, 97, 103, 101, 115 });
            writer.WriteStartArray();
            foreach (CoreChatMessage message in Messages)
            {
                writer.WriteObjectValue(message);
            }
            writer.WriteEndArray();
            if (Optional.IsDefined(Functions) && Functions.Count > 0)
            {
                writer.WritePropertyName(new byte[9] { 102, 117, 110, 99, 116, 105, 111, 110, 115 });
                writer.WriteStartArray();
                foreach (CoreFunctionDefinition function in Functions)
                {
                    if (function.IsPredefined)
                    {
                        throw new ArgumentException("Predefined function definitions such as 'auto' and 'none' cannot be provided as\r\n                            custom functions. These should only be used to constrain the FunctionCall option.");
                    }
                    writer.WriteObjectValue(function);
                }
                writer.WriteEndArray();
            }
            if (Optional.IsDefined(FunctionCall))
            {
                writer.WritePropertyName("function_call");
                if (FunctionCall.IsPredefined)
                {
                    writer.WriteStringValue(FunctionCall.Name);
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(FunctionCall.Name);
                    writer.WriteEndObject();
                }
            }
            if (Optional.IsDefined(MaxTokens))
            {
                if (MaxTokens.HasValue)
                {
                    writer.WritePropertyName(new byte[10] { 109, 97, 120, 95, 116, 111, 107, 101, 110, 115 });
                    writer.WriteNumberValue(MaxTokens.Value);
                }
                else
                {
                    writer.WriteNull("max_tokens");
                }
            }
            if (Optional.IsDefined(Temperature))
            {
                if (Temperature.HasValue)
                {
                    writer.WritePropertyName(new byte[11]
                    {
                    116, 101, 109, 112, 101, 114, 97, 116, 117, 114,
                    101
                    });
                    writer.WriteNumberValue(Temperature.Value);
                }
                else
                {
                    writer.WriteNull("temperature");
                }
            }
            if (Optional.IsDefined(NucleusSamplingFactor))
            {
                if (NucleusSamplingFactor.HasValue)
                {
                    writer.WritePropertyName(new byte[5] { 116, 111, 112, 95, 112 });
                    writer.WriteNumberValue(NucleusSamplingFactor.Value);
                }
                else
                {
                    writer.WriteNull("top_p");
                }
            }
            if (Optional.IsCollectionDefined(TokenSelectionBiases))
            {
                writer.WritePropertyName(new byte[10] { 108, 111, 103, 105, 116, 95, 98, 105, 97, 115 });
                writer.WriteStartObject();
                foreach (KeyValuePair<int, int> tokenSelectionBias in TokenSelectionBiases)
                {
                    writer.WritePropertyName($"{tokenSelectionBias.Key}");
                    writer.WriteNumberValue(tokenSelectionBias.Value);
                }
                writer.WriteEndObject();
            }
            if (Optional.IsDefined(User))
            {
                writer.WritePropertyName(new byte[4] { 117, 115, 101, 114 });
                writer.WriteStringValue(User);
            }
            if (Optional.IsDefined(ChoiceCount))
            {
                if (ChoiceCount.HasValue)
                {
                    writer.WritePropertyName(new byte[1] { 110 });
                    writer.WriteNumberValue(ChoiceCount.Value);
                }
                else
                {
                    writer.WriteNull("n");
                }
            }
            if (Optional.IsCollectionDefined(StopSequences))
            {
                writer.WritePropertyName(new byte[4] { 115, 116, 111, 112 });
                writer.WriteStartArray();
                foreach (string stopSequence in StopSequences)
                {
                    writer.WriteStringValue(stopSequence);
                }
                writer.WriteEndArray();
            }
            if (Optional.IsDefined(PresencePenalty))
            {
                if (PresencePenalty.HasValue)
                {
                    writer.WritePropertyName(new byte[16]
                    {
                    112, 114, 101, 115, 101, 110, 99, 101, 95, 112,
                    101, 110, 97, 108, 116, 121
                    });
                    writer.WriteNumberValue(PresencePenalty.Value);
                }
                else
                {
                    writer.WriteNull("presence_penalty");
                }
            }
            if (Optional.IsDefined(FrequencyPenalty))
            {
                if (FrequencyPenalty.HasValue)
                {
                    writer.WritePropertyName(new byte[17]
                    {
                    102, 114, 101, 113, 117, 101, 110, 99, 121, 95,
                    112, 101, 110, 97, 108, 116, 121
                    });
                    writer.WriteNumberValue(FrequencyPenalty.Value);
                }
                else
                {
                    writer.WriteNull("frequency_penalty");
                }
            }
            if (Optional.IsDefined(InternalShouldStreamResponse))
            {
                if (InternalShouldStreamResponse.HasValue)
                {
                    writer.WritePropertyName(new byte[6] { 115, 116, 114, 101, 97, 109 });
                    writer.WriteBooleanValue(InternalShouldStreamResponse.Value);
                }
                else
                {
                    writer.WriteNull("stream");
                }
            }
            if (Optional.IsDefined(InternalNonAzureModelName))
            {
                writer.WritePropertyName(new byte[5] { 109, 111, 100, 101, 108 });
                writer.WriteStringValue(InternalNonAzureModelName);
            }
            if (AzureExtensionsOptions != null)
            {
                ((IUtf8JsonSerializable)AzureExtensionsOptions).Write(writer);
            }
            writer.WriteEndObject();
        }

        internal CoreChatCompletionsOptions(IList<CoreChatMessage> messages, IList<CoreFunctionDefinition> functions, CoreFunctionDefinition functionCall, int? maxTokens, float? temperature, float? nucleusSamplingFactor, IDictionary<string, int> internalStringKeyedTokenSelectionBiases, string user, int? choiceCount, IList<string> stopSequences, float? presencePenalty, float? frequencyPenalty, bool? internalShouldStreamResponse, string internalNonAzureModelName, IList<AzureChatExtensionConfiguration> internalAzureExtensionsDataSources)
        {
            Messages = messages;
            Functions = functions;
            FunctionCall = functionCall;
            MaxTokens = maxTokens;
            Temperature = temperature;
            NucleusSamplingFactor = nucleusSamplingFactor;
            InternalStringKeyedTokenSelectionBiases = internalStringKeyedTokenSelectionBiases;
            User = user;
            ChoiceCount = choiceCount;
            StopSequences = stopSequences;
            PresencePenalty = presencePenalty;
            FrequencyPenalty = frequencyPenalty;
            InternalShouldStreamResponse = internalShouldStreamResponse;
            InternalNonAzureModelName = internalNonAzureModelName;
            InternalAzureExtensionsDataSources = internalAzureExtensionsDataSources;
        }

        internal virtual RequestContent ToRequestContent()
        {
            Utf8JsonRequestContent utf8JsonRequestContent = new Utf8JsonRequestContent();
            utf8JsonRequestContent.JsonWriter.WriteObjectValue(this);
            return utf8JsonRequestContent;
        }
    }


    public class CoreCompletionsOptions : IUtf8JsonSerializable
    {
        public int? ChoicesPerPrompt { get; set; }

        public bool? Echo { get; set; }

        public float? FrequencyPenalty { get; set; }

        public int? GenerationSampleCount { get; set; }

        public int? LogProbabilityCount { get; set; }

        public int? MaxTokens { get; set; }

        public float? NucleusSamplingFactor { get; set; }

        public float? PresencePenalty { get; set; }

        public IList<string> Prompts { get; }

        public IList<string> StopSequences { get; }

        public float? Temperature { get; set; }

        internal IDictionary<string, int> InternalStringKeyedTokenSelectionBiases { get; }

        public IDictionary<int, int> TokenSelectionBiases { get; }

        internal bool? InternalShouldStreamResponse { get; set; }

        internal string InternalNonAzureModelName { get; set; }

        public string User { get; set; }

        public CoreCompletionsOptions(IEnumerable<string> prompts)
        {
            Azure.Core.Argument.AssertNotNull(prompts, "prompts");
            Prompts = prompts.ToList();
            TokenSelectionBiases = new ChangeTrackingDictionary<int, int>();
            StopSequences = new ChangeTrackingList<string>();
        }

        public CoreCompletionsOptions()
            : this(new ChangeTrackingList<string>())
        {
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            if (Optional.IsCollectionDefined(Prompts) && Prompts.Count > 0)
            {
                writer.WritePropertyName(new byte[6] { 112, 114, 111, 109, 112, 116 });
                writer.WriteStartArray();
                foreach (string prompt in Prompts)
                {
                    writer.WriteStringValue(prompt);
                }
                writer.WriteEndArray();
            }
            if (Optional.IsDefined(MaxTokens))
            {
                if (MaxTokens.HasValue)
                {
                    writer.WritePropertyName(new byte[10] { 109, 97, 120, 95, 116, 111, 107, 101, 110, 115 });
                    writer.WriteNumberValue(MaxTokens.Value);
                }
                else
                {
                    writer.WriteNull("max_tokens");
                }
            }
            if (Optional.IsDefined(Temperature))
            {
                if (Temperature.HasValue)
                {
                    writer.WritePropertyName(new byte[11]
                    {
                    116, 101, 109, 112, 101, 114, 97, 116, 117, 114,
                    101
                    });
                    writer.WriteNumberValue(Temperature.Value);
                }
                else
                {
                    writer.WriteNull("temperature");
                }
            }
            if (Optional.IsDefined(NucleusSamplingFactor))
            {
                if (NucleusSamplingFactor.HasValue)
                {
                    writer.WritePropertyName(new byte[5] { 116, 111, 112, 95, 112 });
                    writer.WriteNumberValue(NucleusSamplingFactor.Value);
                }
                else
                {
                    writer.WriteNull("top_p");
                }
            }
            if (Optional.IsCollectionDefined(TokenSelectionBiases))
            {
                writer.WritePropertyName(new byte[10] { 108, 111, 103, 105, 116, 95, 98, 105, 97, 115 });
                writer.WriteStartObject();
                foreach (KeyValuePair<int, int> tokenSelectionBias in TokenSelectionBiases)
                {
                    writer.WritePropertyName($"{tokenSelectionBias.Key}");
                    writer.WriteNumberValue(tokenSelectionBias.Value);
                }
                writer.WriteEndObject();
            }
            if (Optional.IsDefined(User))
            {
                writer.WritePropertyName(new byte[4] { 117, 115, 101, 114 });
                writer.WriteStringValue(User);
            }
            if (Optional.IsDefined(ChoicesPerPrompt))
            {
                if (ChoicesPerPrompt.HasValue)
                {
                    writer.WritePropertyName(new byte[1] { 110 });
                    writer.WriteNumberValue(ChoicesPerPrompt.Value);
                }
                else
                {
                    writer.WriteNull("n");
                }
            }
            if (Optional.IsDefined(LogProbabilityCount))
            {
                if (LogProbabilityCount.HasValue)
                {
                    writer.WritePropertyName(new byte[8] { 108, 111, 103, 112, 114, 111, 98, 115 });
                    writer.WriteNumberValue(LogProbabilityCount.Value);
                }
                else
                {
                    writer.WriteNull("logprobs");
                }
            }
            if (Optional.IsDefined(Echo))
            {
                if (Echo.HasValue)
                {
                    writer.WritePropertyName(new byte[4] { 101, 99, 104, 111 });
                    writer.WriteBooleanValue(Echo.Value);
                }
                else
                {
                    writer.WriteNull("echo");
                }
            }
            if (Optional.IsCollectionDefined(StopSequences))
            {
                writer.WritePropertyName(new byte[4] { 115, 116, 111, 112 });
                writer.WriteStartArray();
                foreach (string stopSequence in StopSequences)
                {
                    writer.WriteStringValue(stopSequence);
                }
                writer.WriteEndArray();
            }
            if (Optional.IsDefined(PresencePenalty))
            {
                if (PresencePenalty.HasValue)
                {
                    writer.WritePropertyName(new byte[16]
                    {
                    112, 114, 101, 115, 101, 110, 99, 101, 95, 112,
                    101, 110, 97, 108, 116, 121
                    });
                    writer.WriteNumberValue(PresencePenalty.Value);
                }
                else
                {
                    writer.WriteNull("presence_penalty");
                }
            }
            if (Optional.IsDefined(FrequencyPenalty))
            {
                if (FrequencyPenalty.HasValue)
                {
                    writer.WritePropertyName(new byte[17]
                    {
                    102, 114, 101, 113, 117, 101, 110, 99, 121, 95,
                    112, 101, 110, 97, 108, 116, 121
                    });
                    writer.WriteNumberValue(FrequencyPenalty.Value);
                }
                else
                {
                    writer.WriteNull("frequency_penalty");
                }
            }
            if (Optional.IsDefined(GenerationSampleCount))
            {
                if (GenerationSampleCount.HasValue)
                {
                    writer.WritePropertyName(new byte[7] { 98, 101, 115, 116, 95, 111, 102 });
                    writer.WriteNumberValue(GenerationSampleCount.Value);
                }
                else
                {
                    writer.WriteNull("best_of");
                }
            }
            if (Optional.IsDefined(InternalShouldStreamResponse))
            {
                if (InternalShouldStreamResponse.HasValue)
                {
                    writer.WritePropertyName(new byte[6] { 115, 116, 114, 101, 97, 109 });
                    writer.WriteBooleanValue(InternalShouldStreamResponse.Value);
                }
                else
                {
                    writer.WriteNull("stream");
                }
            }
            if (Optional.IsDefined(InternalNonAzureModelName))
            {
                writer.WritePropertyName(new byte[5] { 109, 111, 100, 101, 108 });
                writer.WriteStringValue(InternalNonAzureModelName);
            }
            writer.WriteEndObject();
        }

        internal CoreCompletionsOptions(IList<string> prompts, int? maxTokens, float? temperature, float? nucleusSamplingFactor, IDictionary<string, int> internalStringKeyedTokenSelectionBiases, string user, int? choicesPerPrompt, int? logProbabilityCount, bool? echo, IList<string> stopSequences, float? presencePenalty, float? frequencyPenalty, int? generationSampleCount, bool? internalShouldStreamResponse, string internalNonAzureModelName)
        {
            Prompts = prompts;
            MaxTokens = maxTokens;
            Temperature = temperature;
            NucleusSamplingFactor = nucleusSamplingFactor;
            InternalStringKeyedTokenSelectionBiases = internalStringKeyedTokenSelectionBiases;
            User = user;
            ChoicesPerPrompt = choicesPerPrompt;
            LogProbabilityCount = logProbabilityCount;
            Echo = echo;
            StopSequences = stopSequences;
            PresencePenalty = presencePenalty;
            FrequencyPenalty = frequencyPenalty;
            GenerationSampleCount = generationSampleCount;
            InternalShouldStreamResponse = internalShouldStreamResponse;
            InternalNonAzureModelName = internalNonAzureModelName;
        }

        internal virtual RequestContent ToRequestContent()
        {
            Utf8JsonRequestContent utf8JsonRequestContent = new Utf8JsonRequestContent();
            utf8JsonRequestContent.JsonWriter.WriteObjectValue(this);
            return utf8JsonRequestContent;
        }
    }

}

using Azure.AI.OpenAI;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreAudioTranscriptionOptions : IUtf8JsonSerializable
    {
        public BinaryData AudioData { get; set; }

        internal string InternalNonAzureModelName { get; set; }

        public AudioTranscriptionFormat? ResponseFormat { get; set; }

        public string Language { get; set; }

        public string Prompt { get; set; }

        public float? Temperature { get; set; }

        public CoreAudioTranscriptionOptions()
        {
        }

        internal virtual RequestContent ToRequestContent()
        {
            MultipartFormDataRequestContent multipartFormDataRequestContent = new MultipartFormDataRequestContent();
            multipartFormDataRequestContent.Add(new StringContent(InternalNonAzureModelName), "model");
            multipartFormDataRequestContent.Add(new ByteArrayContent(AudioData.ToArray()), "file", "@file.wav");
            if (Optional.IsDefined(ResponseFormat))
            {
                multipartFormDataRequestContent.Add(new StringContent(ResponseFormat.ToString()), "response_format");
            }
            if (Optional.IsDefined(Prompt))
            {
                multipartFormDataRequestContent.Add(new StringContent(Prompt), "prompt");
            }
            if (Optional.IsDefined(Temperature))
            {
                multipartFormDataRequestContent.Add(new StringContent($"{Temperature}"), "temperature");
            }
            if (Optional.IsDefined(Language))
            {
                multipartFormDataRequestContent.Add(new StringContent(Language), "language");
            }
            return multipartFormDataRequestContent;
        }

        public CoreAudioTranscriptionOptions(BinaryData audioData)
        {
            Azure.Core.Argument.AssertNotNull(audioData, "audioData");
            AudioData = audioData;
        }

        internal CoreAudioTranscriptionOptions(BinaryData audioData, AudioTranscriptionFormat? responseFormat, string language, string prompt, float? temperature, string internalNonAzureModelName)
        {
            AudioData = audioData;
            ResponseFormat = responseFormat;
            Language = language;
            Prompt = prompt;
            Temperature = temperature;
            InternalNonAzureModelName = internalNonAzureModelName;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(new byte[4] { 102, 105, 108, 101 });
            writer.WriteBase64StringValue(AudioData.ToArray(), "D");
            if (Optional.IsDefined(ResponseFormat))
            {
                writer.WritePropertyName(new byte[15]
                {
                114, 101, 115, 112, 111, 110, 115, 101, 95, 102,
                111, 114, 109, 97, 116
                });
                writer.WriteStringValue(ResponseFormat.Value.ToString());
            }
            if (Optional.IsDefined(Language))
            {
                writer.WritePropertyName(new byte[8] { 108, 97, 110, 103, 117, 97, 103, 101 });
                writer.WriteStringValue(Language);
            }
            if (Optional.IsDefined(Prompt))
            {
                writer.WritePropertyName(new byte[6] { 112, 114, 111, 109, 112, 116 });
                writer.WriteStringValue(Prompt);
            }
            if (Optional.IsDefined(Temperature))
            {
                writer.WritePropertyName(new byte[11]
                {
                116, 101, 109, 112, 101, 114, 97, 116, 117, 114,
                101
                });
                writer.WriteNumberValue(Temperature.Value);
            }
            if (Optional.IsDefined(InternalNonAzureModelName))
            {
                writer.WritePropertyName(new byte[5] { 109, 111, 100, 101, 108 });
                writer.WriteStringValue(InternalNonAzureModelName);
            }
            writer.WriteEndObject();
        }
    }


}
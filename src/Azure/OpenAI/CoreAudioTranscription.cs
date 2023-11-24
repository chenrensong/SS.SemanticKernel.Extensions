using Azure.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreAudioTranscription
    {
        internal AudioTaskLabel? InternalAudioTaskLabel { get; }

        public string Text { get; }

        public string Language { get; }

        public TimeSpan? Duration { get; }

        public IReadOnlyList<AudioTranscriptionSegment> Segments { get; }

        internal static CoreAudioTranscription FromResponse(Response response)
        {
            if (response.Headers.ContentType.Contains("text/plain"))
            {
                return new CoreAudioTranscription(response.Content.ToString(), null, null, null, null);
            }
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeAudioTranscription(jsonDocument.RootElement);
            }
        }

        internal CoreAudioTranscription(string text)
        {
            Azure.Core.Argument.AssertNotNull(text, "text");
            Text = text;
            Segments = new ChangeTrackingList<AudioTranscriptionSegment>();
        }

        internal CoreAudioTranscription(string text, AudioTaskLabel? internalAudioTaskLabel, string language, TimeSpan? duration, IReadOnlyList<AudioTranscriptionSegment> segments)
        {
            Text = text;
            InternalAudioTaskLabel = internalAudioTaskLabel;
            Language = language;
            Duration = duration;
            Segments = segments;
        }

        internal static CoreAudioTranscription DeserializeAudioTranscription(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string text = null;
            Optional<AudioTaskLabel> optional = default(Optional<AudioTaskLabel>);
            Optional<string> optional2 = default(Optional<string>);
            Optional<TimeSpan> optional3 = default(Optional<TimeSpan>);
            Optional<IReadOnlyList<AudioTranscriptionSegment>> optional4 = default(Optional<IReadOnlyList<AudioTranscriptionSegment>>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[4] { 116, 101, 120, 116 }))
                {
                    text = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[4] { 116, 97, 115, 107 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional = new AudioTaskLabel(item.Value.GetString());
                    }
                }
                else if (item.NameEquals(new byte[8] { 108, 97, 110, 103, 117, 97, 103, 101 }))
                {
                    optional2 = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[8] { 100, 117, 114, 97, 116, 105, 111, 110 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional3 = TimeSpan.FromSeconds(item.Value.GetDouble());
                    }
                }
                else
                {
                    if (!item.NameEquals(new byte[8] { 115, 101, 103, 109, 101, 110, 116, 115 }) || item.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<AudioTranscriptionSegment> list = new List<AudioTranscriptionSegment>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(AudioTranscriptionSegment.DeserializeAudioTranscriptionSegment(item2));
                    }
                    optional4 = list;
                }
            }
            return new CoreAudioTranscription(text, Optional.ToNullable(optional), optional2.Value, Optional.ToNullable(optional3), Optional.ToList(optional4));
        }
    }


}
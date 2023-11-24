using Azure.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreEmbeddings
    {
        public IReadOnlyList<CoreEmbeddingItem> Data { get; }

        public CoreEmbeddingsUsage Usage { get; }

        internal CoreEmbeddings(IEnumerable<CoreEmbeddingItem> data, CoreEmbeddingsUsage usage)
        {
            Azure.Core.Argument.AssertNotNull(data, "data");
            Azure.Core.Argument.AssertNotNull(usage, "usage");
            Data = data.ToList();
            Usage = usage;
        }

        internal CoreEmbeddings(IReadOnlyList<CoreEmbeddingItem> data, CoreEmbeddingsUsage usage)
        {
            Data = data;
            Usage = usage;
        }

        internal static CoreEmbeddings DeserializeEmbeddings(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            IReadOnlyList<CoreEmbeddingItem> data = null;
            CoreEmbeddingsUsage usage = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[4] { 100, 97, 116, 97 }))
                {
                    List<CoreEmbeddingItem> list = new List<CoreEmbeddingItem>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(CoreEmbeddingItem.DeserializeEmbeddingItem(item2));
                    }
                    data = list;
                }
                else if (item.NameEquals(new byte[5] { 117, 115, 97, 103, 101 }))
                {
                    usage = CoreEmbeddingsUsage.DeserializeEmbeddingsUsage(item.Value);
                }
            }
            return new CoreEmbeddings(data, usage);
        }

        internal static CoreEmbeddings FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeEmbeddings(jsonDocument.RootElement);
            }
        }
    }

    public class CoreAudioTranslation
    {
        internal AudioTaskLabel? InternalAudioTaskLabel { get; }

        public string Text { get; }

        public string Language { get; }

        public TimeSpan? Duration { get; }

        public IReadOnlyList<AudioTranslationSegment> Segments { get; }

        internal static CoreAudioTranslation FromResponse(Response response)
        {
            if (response.Headers.ContentType.Contains("text/plain"))
            {
                return new CoreAudioTranslation(response.Content.ToString(), null, null, null, null);
            }
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeAudioTranslation(jsonDocument.RootElement);
            }
        }

        internal CoreAudioTranslation(string text)
        {
            Azure.Core.Argument.AssertNotNull(text, "text");
            Text = text;
            Segments = new ChangeTrackingList<AudioTranslationSegment>();
        }

        internal CoreAudioTranslation(string text, AudioTaskLabel? internalAudioTaskLabel, string language, TimeSpan? duration, IReadOnlyList<AudioTranslationSegment> segments)
        {
            Text = text;
            InternalAudioTaskLabel = internalAudioTaskLabel;
            Language = language;
            Duration = duration;
            Segments = segments;
        }

        internal static CoreAudioTranslation DeserializeAudioTranslation(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string text = null;
            Optional<AudioTaskLabel> optional = default(Optional<AudioTaskLabel>);
            Optional<string> optional2 = default(Optional<string>);
            Optional<TimeSpan> optional3 = default(Optional<TimeSpan>);
            Optional<IReadOnlyList<AudioTranslationSegment>> optional4 = default(Optional<IReadOnlyList<AudioTranslationSegment>>);
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
                    List<AudioTranslationSegment> list = new List<AudioTranslationSegment>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(AudioTranslationSegment.DeserializeAudioTranslationSegment(item2));
                    }
                    optional4 = list;
                }
            }
            return new CoreAudioTranslation(text, Optional.ToNullable(optional), optional2.Value, Optional.ToNullable(optional3), Optional.ToList(optional4));
        }
    }

    internal struct AudioTaskLabel : IEquatable<AudioTaskLabel>
    {
        private readonly string _value;

        private const string TranscribeValue = "transcribe";

        private const string TranslateValue = "translate";

        public static AudioTaskLabel Transcribe { get; } = new AudioTaskLabel("transcribe");


        public static AudioTaskLabel Translate { get; } = new AudioTaskLabel("translate");


        public AudioTaskLabel(string value)
        {
            _value = value ?? throw new ArgumentNullException("value");
        }

        public static bool operator ==(AudioTaskLabel left, AudioTaskLabel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AudioTaskLabel left, AudioTaskLabel right)
        {
            return !left.Equals(right);
        }

        public static implicit operator AudioTaskLabel(string value)
        {
            return new AudioTaskLabel(value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is AudioTaskLabel other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(AudioTaskLabel other)
        {
            return string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _value;
        }
    }

    public class AudioTranscriptionSegment
    {
        public int Id { get; }

        public TimeSpan Start { get; }

        public TimeSpan End { get; }

        public string Text { get; }

        public float Temperature { get; }

        public float AverageLogProbability { get; }

        public float CompressionRatio { get; }

        public float NoSpeechProbability { get; }

        public IReadOnlyList<int> Tokens { get; }

        public int Seek { get; }

        internal AudioTranscriptionSegment(int id, TimeSpan start, TimeSpan end, string text, float temperature, float averageLogProbability, float compressionRatio, float noSpeechProbability, IEnumerable<int> tokens, int seek)
        {
            Azure.Core.Argument.AssertNotNull(text, "text");
            Azure.Core.Argument.AssertNotNull(tokens, "tokens");
            Id = id;
            Start = start;
            End = end;
            Text = text;
            Temperature = temperature;
            AverageLogProbability = averageLogProbability;
            CompressionRatio = compressionRatio;
            NoSpeechProbability = noSpeechProbability;
            Tokens = tokens.ToList();
            Seek = seek;
        }

        internal AudioTranscriptionSegment(int id, TimeSpan start, TimeSpan end, string text, float temperature, float averageLogProbability, float compressionRatio, float noSpeechProbability, IReadOnlyList<int> tokens, int seek)
        {
            Id = id;
            Start = start;
            End = end;
            Text = text;
            Temperature = temperature;
            AverageLogProbability = averageLogProbability;
            CompressionRatio = compressionRatio;
            NoSpeechProbability = noSpeechProbability;
            Tokens = tokens;
            Seek = seek;
        }

        internal static AudioTranscriptionSegment DeserializeAudioTranscriptionSegment(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            int id = 0;
            TimeSpan start = default(TimeSpan);
            TimeSpan end = default(TimeSpan);
            string text = null;
            float temperature = 0f;
            float averageLogProbability = 0f;
            float compressionRatio = 0f;
            float noSpeechProbability = 0f;
            IReadOnlyList<int> tokens = null;
            int seek = 0;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[2] { 105, 100 }))
                {
                    id = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[5] { 115, 116, 97, 114, 116 }))
                {
                    start = TimeSpan.FromSeconds(item.Value.GetDouble());
                }
                else if (item.NameEquals(new byte[3] { 101, 110, 100 }))
                {
                    end = TimeSpan.FromSeconds(item.Value.GetDouble());
                }
                else if (item.NameEquals(new byte[4] { 116, 101, 120, 116 }))
                {
                    text = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[11]
                {
                116, 101, 109, 112, 101, 114, 97, 116, 117, 114,
                101
                }))
                {
                    temperature = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[11]
                {
                97, 118, 103, 95, 108, 111, 103, 112, 114, 111,
                98
                }))
                {
                    averageLogProbability = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[17]
                {
                99, 111, 109, 112, 114, 101, 115, 115, 105, 111,
                110, 95, 114, 97, 116, 105, 111
                }))
                {
                    compressionRatio = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[14]
                {
                110, 111, 95, 115, 112, 101, 101, 99, 104, 95,
                112, 114, 111, 98
                }))
                {
                    noSpeechProbability = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[6] { 116, 111, 107, 101, 110, 115 }))
                {
                    List<int> list = new List<int>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(item2.GetInt32());
                    }
                    tokens = list;
                }
                else if (item.NameEquals(new byte[4] { 115, 101, 101, 107 }))
                {
                    seek = item.Value.GetInt32();
                }
            }
            return new AudioTranscriptionSegment(id, start, end, text, temperature, averageLogProbability, compressionRatio, noSpeechProbability, tokens, seek);
        }

        internal static AudioTranscriptionSegment FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeAudioTranscriptionSegment(jsonDocument.RootElement);
            }
        }
    }

    public class AudioTranslationSegment
    {
        public int Id { get; }

        public TimeSpan Start { get; }

        public TimeSpan End { get; }

        public string Text { get; }

        public float Temperature { get; }

        public float AverageLogProbability { get; }

        public float CompressionRatio { get; }

        public float NoSpeechProbability { get; }

        public IReadOnlyList<int> Tokens { get; }

        public int Seek { get; }

        internal AudioTranslationSegment(int id, TimeSpan start, TimeSpan end, string text, float temperature, float averageLogProbability, float compressionRatio, float noSpeechProbability, IEnumerable<int> tokens, int seek)
        {
            Azure.Core.Argument.AssertNotNull(text, "text");
            Azure.Core.Argument.AssertNotNull(tokens, "tokens");
            Id = id;
            Start = start;
            End = end;
            Text = text;
            Temperature = temperature;
            AverageLogProbability = averageLogProbability;
            CompressionRatio = compressionRatio;
            NoSpeechProbability = noSpeechProbability;
            Tokens = tokens.ToList();
            Seek = seek;
        }

        internal AudioTranslationSegment(int id, TimeSpan start, TimeSpan end, string text, float temperature, float averageLogProbability, float compressionRatio, float noSpeechProbability, IReadOnlyList<int> tokens, int seek)
        {
            Id = id;
            Start = start;
            End = end;
            Text = text;
            Temperature = temperature;
            AverageLogProbability = averageLogProbability;
            CompressionRatio = compressionRatio;
            NoSpeechProbability = noSpeechProbability;
            Tokens = tokens;
            Seek = seek;
        }

        internal static AudioTranslationSegment DeserializeAudioTranslationSegment(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            int id = 0;
            TimeSpan start = default(TimeSpan);
            TimeSpan end = default(TimeSpan);
            string text = null;
            float temperature = 0f;
            float averageLogProbability = 0f;
            float compressionRatio = 0f;
            float noSpeechProbability = 0f;
            IReadOnlyList<int> tokens = null;
            int seek = 0;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[2] { 105, 100 }))
                {
                    id = item.Value.GetInt32();
                }
                else if (item.NameEquals(new byte[5] { 115, 116, 97, 114, 116 }))
                {
                    start = TimeSpan.FromSeconds(item.Value.GetDouble());
                }
                else if (item.NameEquals(new byte[3] { 101, 110, 100 }))
                {
                    end = TimeSpan.FromSeconds(item.Value.GetDouble());
                }
                else if (item.NameEquals(new byte[4] { 116, 101, 120, 116 }))
                {
                    text = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[11]
                {
                116, 101, 109, 112, 101, 114, 97, 116, 117, 114,
                101
                }))
                {
                    temperature = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[11]
                {
                97, 118, 103, 95, 108, 111, 103, 112, 114, 111,
                98
                }))
                {
                    averageLogProbability = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[17]
                {
                99, 111, 109, 112, 114, 101, 115, 115, 105, 111,
                110, 95, 114, 97, 116, 105, 111
                }))
                {
                    compressionRatio = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[14]
                {
                110, 111, 95, 115, 112, 101, 101, 99, 104, 95,
                112, 114, 111, 98
                }))
                {
                    noSpeechProbability = item.Value.GetSingle();
                }
                else if (item.NameEquals(new byte[6] { 116, 111, 107, 101, 110, 115 }))
                {
                    List<int> list = new List<int>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(item2.GetInt32());
                    }
                    tokens = list;
                }
                else if (item.NameEquals(new byte[4] { 115, 101, 101, 107 }))
                {
                    seek = item.Value.GetInt32();
                }
            }
            return new AudioTranslationSegment(id, start, end, text, temperature, averageLogProbability, compressionRatio, noSpeechProbability, tokens, seek);
        }

        internal static AudioTranslationSegment FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeAudioTranslationSegment(jsonDocument.RootElement);
            }
        }
    }





}

using Azure.AI.OpenAI;
using Azure.Core;
using Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreContentFilterResults
    {
        public ContentFilterResult Sexual { get; }

        public ContentFilterResult Violence { get; }

        public ContentFilterResult Hate { get; }

        public ContentFilterResult SelfHarm { get; }

        public ResponseError Error { get; }

        internal CoreContentFilterResults()
        {
        }

        internal CoreContentFilterResults(ContentFilterResult sexual, ContentFilterResult violence, ContentFilterResult hate, ContentFilterResult selfHarm, ResponseError error)
        {
            Sexual = sexual;
            Violence = violence;
            Hate = hate;
            SelfHarm = selfHarm;
            Error = error;
        }

        internal static CoreContentFilterResults DeserializeContentFilterResults(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            Optional<ContentFilterResult> optional = default(Optional<ContentFilterResult>);
            Optional<ContentFilterResult> optional2 = default(Optional<ContentFilterResult>);
            Optional<ContentFilterResult> optional3 = default(Optional<ContentFilterResult>);
            Optional<ContentFilterResult> optional4 = default(Optional<ContentFilterResult>);
            Optional<ResponseError> optional5 = default(Optional<ResponseError>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[6] { 115, 101, 120, 117, 97, 108 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional = ContentFilterResult.DeserializeContentFilterResult(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[8] { 118, 105, 111, 108, 101, 110, 99, 101 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional2 = ContentFilterResult.DeserializeContentFilterResult(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[4] { 104, 97, 116, 101 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional3 = ContentFilterResult.DeserializeContentFilterResult(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[9] { 115, 101, 108, 102, 95, 104, 97, 114, 109 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional4 = ContentFilterResult.DeserializeContentFilterResult(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[5] { 101, 114, 114, 111, 114 }) && item.Value.ValueKind != JsonValueKind.Null)
                {
                    optional5 = JsonSerializer.Deserialize<ResponseError>(item.Value.GetRawText());
                }
            }
            return new CoreContentFilterResults(optional.Value, optional2.Value, optional3.Value, optional4.Value, optional5.Value);
        }

        internal static CoreContentFilterResults FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeContentFilterResults(jsonDocument.RootElement);
            }
        }
    }

    public class ContentFilterResult
    {
        public ContentFilterSeverity Severity { get; }

        public bool Filtered { get; }

        internal ContentFilterResult(ContentFilterSeverity severity, bool filtered)
        {
            Severity = severity;
            Filtered = filtered;
        }

        internal static ContentFilterResult DeserializeContentFilterResult(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            ContentFilterSeverity severity = default(ContentFilterSeverity);
            bool filtered = false;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[8] { 115, 101, 118, 101, 114, 105, 116, 121 }))
                {
                    severity = new ContentFilterSeverity(item.Value.GetString());
                }
                else if (item.NameEquals(new byte[8] { 102, 105, 108, 116, 101, 114, 101, 100 }))
                {
                    filtered = item.Value.GetBoolean();
                }
            }
            return new ContentFilterResult(severity, filtered);
        }

        internal static ContentFilterResult FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeContentFilterResult(jsonDocument.RootElement);
            }
        }
    }


}

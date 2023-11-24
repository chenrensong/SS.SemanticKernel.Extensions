using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure;
using Azure.Core;

namespace Azure.AI.OpenAI
{
    public partial class ImageGenerations
    {
        public IReadOnlyList<CoreImageLocation> Data { get; }

        public DateTimeOffset Created { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DeserializeDataProperty(JsonProperty property, ref IReadOnlyList<CoreImageLocation> data)
        {
            List<CoreImageLocation> list = new List<CoreImageLocation>();
            foreach (JsonElement item in property.Value.EnumerateArray())
            {
                list.Add(CoreImageLocation.DeserializeImageLocation(item));
            }
            data = list;
        }

        internal ImageGenerations(DateTimeOffset created, IEnumerable<CoreImageLocation> data)
        {
            Azure.Core.Argument.AssertNotNull(data, "data");
            Created = created;
            Data = data.ToList();
        }

        internal ImageGenerations(DateTimeOffset created, IReadOnlyList<CoreImageLocation> data)
        {
            Created = created;
            Data = data;
        }

        internal static ImageGenerations DeserializeImageGenerations(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            DateTimeOffset created = default(DateTimeOffset);
            IReadOnlyList<CoreImageLocation> data = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[7] { 99, 114, 101, 97, 116, 101, 100 }))
                {
                    created = DateTimeOffset.FromUnixTimeSeconds(item.Value.GetInt64());
                }
                else if (item.NameEquals(new byte[4] { 100, 97, 116, 97 }))
                {
                    DeserializeDataProperty(item, ref data);
                }
            }
            return new ImageGenerations(created, data);
        }

        internal static ImageGenerations FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeImageGenerations(jsonDocument.RootElement);
            }
        }
    }
}
using System;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public partial class CoreImageLocation
    {
        public Uri Url { get; }

        internal CoreImageLocation(Uri url)
        {
            Azure.Core.Argument.AssertNotNull(url, "url");
            Url = url;
        }

        internal static CoreImageLocation DeserializeImageLocation(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            Uri url = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[3] { 117, 114, 108 }))
                {
                    url = new Uri(item.Value.GetString());
                }
            }
            return new CoreImageLocation(url);
        }

        internal static CoreImageLocation FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeImageLocation(jsonDocument.RootElement);
            }
        }
    }





}
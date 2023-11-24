using Azure.Core;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreFunctionCall : IUtf8JsonSerializable
    {
        public string Name { get; set; }

        public string Arguments { get; set; }

        public CoreFunctionCall(string name, string arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(new byte[4] { 110, 97, 109, 101 });
            writer.WriteStringValue(Name);
            writer.WritePropertyName(new byte[9] { 97, 114, 103, 117, 109, 101, 110, 116, 115 });
            writer.WriteStringValue(Arguments);
            writer.WriteEndObject();
        }

        internal static CoreFunctionCall DeserializeFunctionCall(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string name = null;
            string arguments = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[4] { 110, 97, 109, 101 }))
                {
                    name = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[9] { 97, 114, 103, 117, 109, 101, 110, 116, 115 }))
                {
                    arguments = item.Value.GetString();
                }
            }
            return new CoreFunctionCall(name, arguments);
        }

        internal static CoreFunctionCall FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeFunctionCall(jsonDocument.RootElement);
            }
        }

        internal virtual RequestContent ToRequestContent()
        {
            Utf8JsonRequestContent utf8JsonRequestContent = new Utf8JsonRequestContent();
            utf8JsonRequestContent.JsonWriter.WriteObjectValue(this);
            return utf8JsonRequestContent;
        }
    }


}

using Azure.Core;
using System.Collections.Generic;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreAzureChatExtensionsMessageContext : IUtf8JsonSerializable
    {
        public IList<CoreChatMessage> Messages { get; }

        public CoreAzureChatExtensionsMessageContext()
        {
            Messages = new ChangeTrackingList<CoreChatMessage>();
        }

        internal CoreAzureChatExtensionsMessageContext(IList<CoreChatMessage> messages)
        {
            Messages = messages;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            if (Optional.IsCollectionDefined(Messages))
            {
                writer.WritePropertyName(new byte[8] { 109, 101, 115, 115, 97, 103, 101, 115 });
                writer.WriteStartArray();
                foreach (CoreChatMessage message in Messages)
                {
                    writer.WriteObjectValue(message);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        internal static CoreAzureChatExtensionsMessageContext DeserializeAzureChatExtensionsMessageContext(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            Optional<IList<CoreChatMessage>> optional = default(Optional<IList<CoreChatMessage>>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (!item.NameEquals(new byte[8] { 109, 101, 115, 115, 97, 103, 101, 115 }) || item.Value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }
                List<CoreChatMessage> list = new List<CoreChatMessage>();
                foreach (JsonElement item2 in item.Value.EnumerateArray())
                {
                    list.Add(CoreChatMessage.DeserializeChatMessage(item2));
                }
                optional = list;
            }
            return new CoreAzureChatExtensionsMessageContext(Optional.ToList(optional));
        }

        internal static CoreAzureChatExtensionsMessageContext FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeAzureChatExtensionsMessageContext(jsonDocument.RootElement);
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

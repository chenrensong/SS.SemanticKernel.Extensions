using Azure.Core;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreChatMessage : IUtf8JsonSerializable
    {
        public ChatRole Role { get; set; }

        public string Content { get; set; }

        public string Name { get; set; }

        public CoreFunctionCall FunctionCall { get; set; }

        public CoreAzureChatExtensionsMessageContext AzureExtensionsContext { get; set; }

        public CoreChatMessage()
        {
        }

        public CoreChatMessage(ChatRole role, string content)
        {
            Role = role;
            Content = content;
        }

        internal CoreChatMessage(ChatRole role, string content, string name, CoreFunctionCall functionCall, CoreAzureChatExtensionsMessageContext azureExtensionsContext)
        {
            Role = role;
            Content = content;
            Name = name;
            FunctionCall = functionCall;
            AzureExtensionsContext = azureExtensionsContext;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(new byte[4] { 114, 111, 108, 101 });
            writer.WriteStringValue(Role.ToString());
            if (Content != null)
            {
                writer.WritePropertyName(new byte[7] { 99, 111, 110, 116, 101, 110, 116 });
                writer.WriteStringValue(Content);
            }
            else
            {
                writer.WriteNull("content");
            }
            if (Optional.IsDefined(Name))
            {
                writer.WritePropertyName(new byte[4] { 110, 97, 109, 101 });
                writer.WriteStringValue(Name);
            }
            if (Optional.IsDefined(FunctionCall))
            {
                writer.WritePropertyName(new byte[13]
                {
                102, 117, 110, 99, 116, 105, 111, 110, 95, 99,
                97, 108, 108
                });
                writer.WriteObjectValue(FunctionCall);
            }
            if (Optional.IsDefined(AzureExtensionsContext))
            {
                writer.WritePropertyName(new byte[7] { 99, 111, 110, 116, 101, 120, 116 });
                writer.WriteObjectValue(AzureExtensionsContext);
            }
            writer.WriteEndObject();
        }

        internal static CoreChatMessage DeserializeChatMessage(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            ChatRole role = default(ChatRole);
            string content = null;
            Optional<string> optional = default(Optional<string>);
            Optional<CoreFunctionCall> optional2 = default(Optional<CoreFunctionCall>);
            Optional<CoreAzureChatExtensionsMessageContext> optional3 = default(Optional<CoreAzureChatExtensionsMessageContext>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[4] { 114, 111, 108, 101 }))
                {
                    role = new ChatRole(item.Value.GetString());
                }
                else if (item.NameEquals(new byte[7] { 99, 111, 110, 116, 101, 110, 116 }))
                {
                    content = ((item.Value.ValueKind != JsonValueKind.Null) ? item.Value.GetString() : null);
                }
                else if (item.NameEquals(new byte[4] { 110, 97, 109, 101 }))
                {
                    optional = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[13]
                {
                102, 117, 110, 99, 116, 105, 111, 110, 95, 99,
                97, 108, 108
                }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional2 = CoreFunctionCall.DeserializeFunctionCall(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[7] { 99, 111, 110, 116, 101, 120, 116 }) && item.Value.ValueKind != JsonValueKind.Null)
                {
                    optional3 = CoreAzureChatExtensionsMessageContext.DeserializeAzureChatExtensionsMessageContext(item.Value);
                }
            }
            return new CoreChatMessage(role, content, optional.Value, optional2.Value, optional3.Value);
        }

        internal static CoreChatMessage FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeChatMessage(jsonDocument.RootElement);
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

using Azure.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreEmbeddingsOptions : IUtf8JsonSerializable
    {
        internal string InternalNonAzureModelName { get; set; }

        public string User { get; set; }

        public IList<string> Input { get; }

        public CoreEmbeddingsOptions(string input)
            : this(new string[1] { input })
        {
        }

        public CoreEmbeddingsOptions()
        {
        }

        public CoreEmbeddingsOptions(IEnumerable<string> input)
        {
            Azure.Core.Argument.AssertNotNull(input, "input");
            Input = input.ToList();
        }

        internal CoreEmbeddingsOptions(string user, string internalNonAzureModelName, IList<string> input)
        {
            User = user;
            InternalNonAzureModelName = internalNonAzureModelName;
            Input = input;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            if (Optional.IsDefined(User))
            {
                writer.WritePropertyName(new byte[4] { 117, 115, 101, 114 });
                writer.WriteStringValue(User);
            }
            if (Optional.IsDefined(InternalNonAzureModelName))
            {
                writer.WritePropertyName(new byte[5] { 109, 111, 100, 101, 108 });
                writer.WriteStringValue(InternalNonAzureModelName);
            }
            writer.WritePropertyName(new byte[5] { 105, 110, 112, 117, 116 });
            writer.WriteStartArray();
            foreach (string item in Input)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        internal virtual RequestContent ToRequestContent()
        {
            Utf8JsonRequestContent utf8JsonRequestContent = new Utf8JsonRequestContent();
            utf8JsonRequestContent.JsonWriter.WriteObjectValue(this);
            return utf8JsonRequestContent;
        }
    }

}

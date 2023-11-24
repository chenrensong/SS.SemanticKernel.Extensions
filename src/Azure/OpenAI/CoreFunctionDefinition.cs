using Azure.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreFunctionDefinition : IUtf8JsonSerializable
    {
        public static CoreFunctionDefinition Auto = CreatePredefinedFunctionDefinition(CoreFunctionCallPreset.Auto.ToString());

        public static CoreFunctionDefinition None = CreatePredefinedFunctionDefinition(CoreFunctionCallPreset.None.ToString());

        public string Name { get; set; }

        internal bool IsPredefined { get; set; }

        public string Description { get; set; }

        public BinaryData Parameters { get; set; }

        public CoreFunctionDefinition()
        {
        }

        internal static CoreFunctionDefinition CreatePredefinedFunctionDefinition(string functionName)
        {
            return new CoreFunctionDefinition(functionName)
            {
                IsPredefined = true
            };
        }

        public CoreFunctionDefinition(string name)
        {
            Azure.Core.Argument.AssertNotNull(name, "name");
            Name = name;
        }

        internal CoreFunctionDefinition(string name, string description, BinaryData parameters)
        {
            Name = name;
            Description = description;
            Parameters = parameters;
        }

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(new byte[4] { 110, 97, 109, 101 });
            writer.WriteStringValue(Name);
            if (Optional.IsDefined(Description))
            {
                writer.WritePropertyName(new byte[11]
                {
                100, 101, 115, 99, 114, 105, 112, 116, 105, 111,
                110
                });
                writer.WriteStringValue(Description);
            }
            if (Optional.IsDefined(Parameters))
            {
                writer.WritePropertyName(new byte[10] { 112, 97, 114, 97, 109, 101, 116, 101, 114, 115 });
                JsonSerializer.Serialize(writer, JsonDocument.Parse(Parameters.ToString()).RootElement);
            }
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

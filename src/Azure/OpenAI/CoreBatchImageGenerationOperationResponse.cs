using Azure.AI.OpenAI;
using Azure;
using System;
using System.Collections.Generic;
using System.Text;
using static UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor.ContentOrderTextExtractor;
using System.Text.Json;
using Azure.Core;

namespace Azure.AI.OpenAI
{
    internal class CoreBatchImageGenerationOperationResponse
    {
        public string Id { get; }

        public DateTimeOffset Created { get; }

        public long? Expires { get; }

        public ImageGenerations Result { get; }

        public AzureOpenAIOperationState Status { get; }

        public ResponseError Error { get; }

        internal CoreBatchImageGenerationOperationResponse(string id, DateTimeOffset created, AzureOpenAIOperationState status)
        {
            Azure.Core.Argument.AssertNotNull(id, "id");
            Id = id;
            Created = created;
            Status = status;
        }

        internal CoreBatchImageGenerationOperationResponse(string id, DateTimeOffset created, long? expires, ImageGenerations result, AzureOpenAIOperationState status, ResponseError error)
        {
            Id = id;
            Created = created;
            Expires = expires;
            Result = result;
            Status = status;
            Error = error;
        }

        internal static CoreBatchImageGenerationOperationResponse DeserializeBatchImageGenerationOperationResponse(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string id = null;
            DateTimeOffset created = default(DateTimeOffset);
            Optional<long> optional = default(Optional<long>);
            Optional<ImageGenerations> optional2 = default(Optional<ImageGenerations>);
            AzureOpenAIOperationState status = default(AzureOpenAIOperationState);
            Optional<ResponseError> optional3 = default(Optional<ResponseError>);
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[2] { 105, 100 }))
                {
                    id = item.Value.GetString();
                }
                else if (item.NameEquals(new byte[7] { 99, 114, 101, 97, 116, 101, 100 }))
                {
                    created = DateTimeOffset.FromUnixTimeSeconds(item.Value.GetInt64());
                }
                else if (item.NameEquals(new byte[7] { 101, 120, 112, 105, 114, 101, 115 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional = item.Value.GetInt64();
                    }
                }
                else if (item.NameEquals(new byte[6] { 114, 101, 115, 117, 108, 116 }))
                {
                    if (item.Value.ValueKind != JsonValueKind.Null)
                    {
                        optional2 = ImageGenerations.DeserializeImageGenerations(item.Value);
                    }
                }
                else if (item.NameEquals(new byte[6] { 115, 116, 97, 116, 117, 115 }))
                {
                    status = new AzureOpenAIOperationState(item.Value.GetString());
                }
                else if (item.NameEquals(new byte[5] { 101, 114, 114, 111, 114 }) && item.Value.ValueKind != JsonValueKind.Null)
                {
                    optional3 = JsonSerializer.Deserialize<ResponseError>(item.Value.GetRawText());
                }
            }
            return new CoreBatchImageGenerationOperationResponse(id, created, Optional.ToNullable(optional), optional2.Value, status, optional3.Value);
        }

        internal static CoreBatchImageGenerationOperationResponse FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeBatchImageGenerationOperationResponse(jsonDocument.RootElement);
            }
        }
    }

}

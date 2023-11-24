using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel
{
    public class CoreTextEmbeddingGeneration : ITextEmbeddingGeneration
    {

        public CoreTextEmbeddingGeneration(string modelId, string endpoint, string key, HttpClient? httpClient = null, ILogger? logger = null)
        {
            _modelId = modelId;
            _endpoint = endpoint;
            _key = key;
            _logger = logger;
            _httpClient = httpClient;
        }

        private string _modelId;

        private string _endpoint;

        private string _key;

        private HttpClient? _httpClient;

        private ILogger? _logger;

        public IReadOnlyDictionary<string, string> Attributes => throw new NotImplementedException();


        private async Task<CoreEmbeddings> InternalGetEmbeddingsAsync(string ModelId, string input,
              CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new
            {
                input = input,
                model = ModelId
            };
            if (!string.IsNullOrEmpty(_key))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");
            }
            var json = JsonSerializer.Serialize(request);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, data, cancellationToken);
            string content = await response.Content.ReadAsStringAsync();
            return CoreEmbeddings.FromResponse(content);
        }

        async Task<IList<ReadOnlyMemory<float>>> IEmbeddingGeneration<string, float>.GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken)
        {

            var result = new List<ReadOnlyMemory<float>>(data.Count);
            foreach (string text in data)
            {
                var options = new EmbeddingsOptions(text);

                var response = await InternalGetEmbeddingsAsync(_modelId, text, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                if (response is null)
                {
                    throw new SKException("Text embedding null response");
                }

                if (response.Data.Count == 0)
                {
                    throw new SKException("Text embedding not found");
                }

                result.Add(response.Data[0].Embedding.ToArray());
            }

            return result;
        }
    }
}

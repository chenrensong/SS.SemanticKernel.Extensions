using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.Embeddings;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.SemanticKernel.AI.AIException;

namespace Microsoft.SemanticKernel {
    public class CoreTextEmbeddingGeneration : ITextEmbeddingGeneration {

        public CoreTextEmbeddingGeneration(string modelId, string endpoint, HttpClient? httpClient = null, ILogger? logger = null) {
            _modelId = modelId;
            _endpoint = endpoint;
            _logger = logger;
            _httpClient = httpClient;
        }

        private string _modelId;

        private string _endpoint;

        private HttpClient? _httpClient;

        private ILogger? _logger;

        public async Task<IList<Embedding<float>>> GenerateEmbeddingsAsync(IList<string> data,
            CancellationToken cancellationToken = default) {
            List<Embedding<float>> result = new List<Embedding<float>>();
            foreach (string text in data) {
                var response = await InternalGetEmbeddingsAsync(_modelId, text, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                if (response == null || (response.Data.Count < 1)) {
                    throw new AIException(ErrorCodes.InvalidResponseContent, "Text embedding not found");
                }
                result.Add(new Embedding<float>(response.Data[0].Embedding, true));
            }
            return result;
        }

        private async Task<CoreEmbeddings> InternalGetEmbeddingsAsync(string ModelId, string input,
              CancellationToken cancellationToken = default(CancellationToken)) {
            var request = new {
                input = input,
                model = ModelId
            };
            var json = JsonSerializer.Serialize(request);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, data, cancellationToken);
            string content = await response.Content.ReadAsStringAsync();
            return CoreEmbeddings.FromResponse(content);
        }
    }
}

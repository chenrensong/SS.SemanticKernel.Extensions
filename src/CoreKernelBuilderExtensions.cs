using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.Embeddings;
using System.Net.Http;

namespace Microsoft.SemanticKernel {
    public static class CoreKernelBuilderExtensions {
        public static KernelBuilder WithCoreTextEmbeddingGenerationService(this KernelBuilder builder,
        string modelId,
        string endpoint,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null) {
            builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (parameters) =>
                new CoreTextEmbeddingGeneration(
                    modelId,
                    endpoint,
                    GetHttpClient(parameters.Config, httpClient, parameters.Logger),
                    parameters.Logger),
                setAsDefault);
            return builder;
        }
        private static HttpClient GetHttpClient(KernelConfig config, HttpClient? httpClient, ILogger? logger) {
            if (httpClient == null) {
                var retryHandler = config.HttpHandlerFactory.Create(logger);
                retryHandler.InnerHandler = NonDisposableCoreHttpClientHandler.Instance;
                return new HttpClient(retryHandler, false); // We should refrain from disposing the underlying SK default HttpClient handler as it would impact other HTTP clients that utilize the same handler.
            }

            return httpClient;
        }
    }
}

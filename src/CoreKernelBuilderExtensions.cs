using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.TextCompletion;
using System;
using System.Net.Http;

namespace Microsoft.SemanticKernel
{
    public static class CoreKernelBuilderExtensions
    {
        public static KernelBuilder WithErnieBotChatCompletionService(this KernelBuilder builder,
            string modelId, string endpoint, string key, string? serviceId = null,
            bool alsoAsTextCompletion = true, bool setAsDefault = false, HttpClient? httpClient = null)
        {
            builder.WithAIService(serviceId, new Func<(ILogger, KernelConfig), IChatCompletion>(Factory), setAsDefault);
            if (alsoAsTextCompletion && typeof(ITextCompletion)!.IsAssignableFrom(typeof(ErnieBotChatCompletion)))
            {
                builder.WithAIService(serviceId, new Func<(ILogger, KernelConfig), ITextCompletion>(Factory), setAsDefault);
            }

            return builder;
            ErnieBotChatCompletion Factory((ILogger Logger, KernelConfig Config) parameters)
            {
                return new ErnieBotChatCompletion(modelId, endpoint, key, GetHttpClient(parameters.Config, httpClient, parameters.Logger), parameters.Logger);
            }
        }

        public static KernelBuilder WithCoreChatCompletionService(this KernelBuilder builder,
            string modelId, string endpoint, string key, string? serviceId = null,
            bool alsoAsTextCompletion = true, bool setAsDefault = false, HttpClient? httpClient = null)
        {
            builder.WithAIService(serviceId, new Func<(ILogger, KernelConfig), IChatCompletion>(Factory), setAsDefault);
            if (alsoAsTextCompletion && typeof(ITextCompletion)!.IsAssignableFrom(typeof(CoreChatCompletion)))
            {
                builder.WithAIService(serviceId, new Func<(ILogger, KernelConfig), ITextCompletion>(Factory), setAsDefault);
            }

            return builder;
            CoreChatCompletion Factory((ILogger Logger, KernelConfig Config) parameters)
            {
                return new CoreChatCompletion(modelId, endpoint, key, GetHttpClient(parameters.Config, httpClient, parameters.Logger), parameters.Logger);
            }
        }

        public static KernelBuilder WithCoreTextEmbeddingGenerationService(this KernelBuilder builder,
            string modelId,
            string endpoint,
            string key,
            string? serviceId = null,
            bool setAsDefault = false,
            HttpClient? httpClient = null)
        {

            builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (parameters) =>
                new CoreTextEmbeddingGeneration(
                    modelId,
                    endpoint,
                    key,
                    GetHttpClient(parameters.Config, httpClient, parameters.Logger),
                    parameters.Logger),
                setAsDefault);
            return builder;
        }


        private static HttpClient GetHttpClient(KernelConfig config, HttpClient? httpClient, ILogger? logger)
        {
            if (httpClient == null)
            {
                var retryHandler = config.HttpHandlerFactory.Create(logger);
                retryHandler.InnerHandler = NonDisposableCoreHttpClientHandler.Instance;
                return new HttpClient(retryHandler, false); // We should refrain from disposing the underlying SK default HttpClient handler as it would impact other HTTP clients that utilize the same handler.
            }

            return httpClient;
        }
    }
}

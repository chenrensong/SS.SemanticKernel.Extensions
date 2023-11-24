﻿// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Azure.Core;
using Microsoft.SemanticKernel.Connectors.AI.Custom.TextEmbedding;
using Microsoft.SemanticKernel.Plugins.Memory;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom;

/// <summary>
/// Provides extension methods for the <see cref="MemoryBuilder"/> class to configure OpenAI and AzureOpenAI connectors.
/// </summary>
public static class OpenAIMemoryBuilderExtensions
{
    /// <summary>
    /// Adds an Azure OpenAI text embeddings service.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="MemoryBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static MemoryBuilder WithCoreAzureOpenAITextEmbeddingGenerationService(
        this MemoryBuilder builder,
        string deploymentName,
        string endpoint,
        string apiKey,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithTextEmbeddingGeneration((loggerFactory, httpHandlerFactory) =>
            new AzureOpenAITextEmbeddingGeneration(
                deploymentName,
                endpoint,
                apiKey,
                modelId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory));

        return builder;
    }

    /// <summary>
    /// Adds an Azure OpenAI text embeddings service.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="MemoryBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credential">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static MemoryBuilder WithCoreAzureOpenAITextEmbeddingGenerationService(
        this MemoryBuilder builder,
        string deploymentName,
        string endpoint,
        TokenCredential credential,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithTextEmbeddingGeneration((loggerFactory, httpHandlerFactory) =>
            new AzureOpenAITextEmbeddingGeneration(
                deploymentName,
                endpoint,
                credential,
                modelId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory));

        return builder;
    }

    /// <summary>
    /// Adds the OpenAI text embeddings service.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="MemoryBuilder"/> instance</param>
    /// <param name="modelId">OpenAI model name, see https://platform.openai.com/docs/models</param>
    /// <param name="apiKey">OpenAI API key, see https://platform.openai.com/account/api-keys</param>
    /// <param name="orgId">OpenAI organization id. This is usually optional unless your account belongs to multiple organizations.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static MemoryBuilder WithCoreOpenAITextEmbeddingGenerationService(
        this MemoryBuilder builder,
        string modelId,
        string endpoint,
        string apiKey,
        string? orgId = null,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithTextEmbeddingGeneration((loggerFactory, httpHandlerFactory) =>
            new OpenAITextEmbeddingGeneration(
                modelId,
                endpoint,
                apiKey,
                orgId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory));

        return builder;
    }
}

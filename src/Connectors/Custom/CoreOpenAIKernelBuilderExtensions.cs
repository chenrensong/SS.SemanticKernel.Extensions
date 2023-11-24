﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.ImageGeneration;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.Custom.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.Custom.ChatCompletionWithData;
using Microsoft.SemanticKernel.Connectors.AI.Custom.ImageGeneration;
using Microsoft.SemanticKernel.Connectors.AI.Custom.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.Custom.TextEmbedding;
using Microsoft.SemanticKernel.Http;

#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace - Using NS of KernelConfig
namespace Microsoft.SemanticKernel;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for the <see cref="KernelBuilder"/> class to configure OpenAI and AzureOpenAI connectors.
/// </summary>
public static class OpenAIKernelBuilderExtensions
{
    #region Text Completion

    /// <summary>
    /// Adds an Azure OpenAI text completion service to the list.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureTextCompletionService(this KernelBuilder builder,
        string deploymentName,
        string endpoint,
        string apiKey,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextCompletion>(serviceId, (loggerFactory, httpHandlerFactory) =>
        {
            var client = CreateAzureOpenAIClient(loggerFactory, httpHandlerFactory, deploymentName, endpoint, new AzureKeyCredential(apiKey), httpClient);
            return new AzureTextCompletion(deploymentName, client, modelId, loggerFactory);
        }, setAsDefault);

        return builder;
    }

    /// <summary>
    /// Adds an Azure OpenAI text completion service to the list.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credentials">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureTextCompletionService(this KernelBuilder builder,
        string deploymentName,
        string endpoint,
        TokenCredential credentials,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextCompletion>(serviceId, (loggerFactory, httpHandlerFactory) =>
        {
            var client = CreateAzureOpenAIClient(loggerFactory, httpHandlerFactory, deploymentName, endpoint, credentials, httpClient);
            return new AzureTextCompletion(deploymentName, client, modelId, loggerFactory);
        }, setAsDefault);

        return builder;
    }

    /// <summary>
    /// Adds an Azure OpenAI text completion service to the list.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="openAIClient">Custom <see cref="OpenAIClient"/>.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureTextCompletionService(this KernelBuilder builder,
        string deploymentName,
        CoreOpenAIClient openAIClient,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false)
    {
        builder.WithAIService<ITextCompletion>(serviceId, (loggerFactory) =>
            new AzureTextCompletion(
                deploymentName,
                openAIClient,
                modelId,
                loggerFactory),
            setAsDefault);

        return builder;
    }

    /// <summary>
    /// Adds the OpenAI text completion service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="modelId">OpenAI model name, see https://platform.openai.com/docs/models</param>
    /// <param name="apiKey">OpenAI API key, see https://platform.openai.com/account/api-keys</param>
    /// <param name="orgId">OpenAI organization id. This is usually optional unless your account belongs to multiple organizations.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreOpenAITextCompletionService(this KernelBuilder builder,
        string modelId,
        string endpoint,
        string apiKey,
        string? orgId = null,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextCompletion>(serviceId, (loggerFactory, httpHandlerFactory) =>
            new CoreOpenAITextCompletion(
                modelId,
                endpoint,
                apiKey,
                orgId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory),
            setAsDefault);
        return builder;
    }

    #endregion

    #region Text Embedding

    /// <summary>
    /// Adds an Azure OpenAI text embeddings service to the list.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAITextEmbeddingGenerationService(this KernelBuilder builder,
        string deploymentName,
        string endpoint,
        string apiKey,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (loggerFactory, httpHandlerFactory) =>
            new AzureOpenAITextEmbeddingGeneration(
                deploymentName,
                endpoint,
                apiKey,
                modelId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory),
            setAsDefault);
        return builder;
    }

    /// <summary>
    /// Adds an Azure OpenAI text embeddings service to the list.
    /// See https://learn.microsoft.com/azure/cognitive-services/openai for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credential">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAITextEmbeddingGenerationService(this KernelBuilder builder,
        string deploymentName,
        string endpoint,
        TokenCredential credential,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (loggerFactory, httpHandlerFactory) =>
            new AzureOpenAITextEmbeddingGeneration(
                deploymentName,
                endpoint,
                credential,
                modelId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory),
            setAsDefault);
        return builder;
    }

    /// <summary>
    /// Adds the OpenAI text embeddings service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="modelId">OpenAI model name, see https://platform.openai.com/docs/models</param>
    /// <param name="apiKey">OpenAI API key, see https://platform.openai.com/account/api-keys</param>
    /// <param name="orgId">OpenAI organization id. This is usually optional unless your account belongs to multiple organizations.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreOpenAITextEmbeddingGenerationService(this KernelBuilder builder,
        string modelId,
        string endpoint,
        string apiKey,
        string? orgId = null,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (loggerFactory, httpHandlerFactory) =>
            new OpenAITextEmbeddingGeneration(
                modelId,
                endpoint,
                apiKey,
                orgId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory),
            setAsDefault);
        return builder;
    }

    #endregion

    #region Chat Completion

    /// <summary>
    /// Adds the Azure OpenAI ChatGPT completion service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="alsoAsTextCompletion">Whether to use the service also for text completion, if supported</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAIChatCompletionService(this KernelBuilder builder,
        string deploymentName,
        string endpoint,
        string apiKey,
        bool alsoAsTextCompletion = true,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        AzureOpenAIChatCompletion Factory(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory)
        {
            var client = CreateAzureOpenAIClient(loggerFactory, httpHandlerFactory, deploymentName, endpoint, new AzureKeyCredential(apiKey), httpClient);

            return new(deploymentName, client, modelId, loggerFactory);
        };

        builder.WithAIService<IChatCompletion>(serviceId, Factory, setAsDefault);

        // If the class implements the text completion interface, allow to use it also for semantic functions
        if (alsoAsTextCompletion && typeof(ITextCompletion).IsAssignableFrom(typeof(AzureOpenAIChatCompletion)))
        {
            builder.WithAIService<ITextCompletion>(serviceId, Factory, setAsDefault);
        }

        return builder;
    }

    /// <summary>
    /// Adds the Azure OpenAI ChatGPT completion service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credentials">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="alsoAsTextCompletion">Whether to use the service also for text completion, if supported</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAIChatCompletionService(this KernelBuilder builder,
        string deploymentName,
        string endpoint,
        TokenCredential credentials,
        bool alsoAsTextCompletion = true,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        AzureOpenAIChatCompletion Factory(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory)
        {
            var client = CreateAzureOpenAIClient(loggerFactory, httpHandlerFactory, deploymentName, endpoint, credentials, httpClient);

            return new(deploymentName, client, modelId, loggerFactory);
        };

        builder.WithAIService<IChatCompletion>(serviceId, Factory, setAsDefault);

        // If the class implements the text completion interface, allow to use it also for semantic functions
        if (alsoAsTextCompletion && typeof(ITextCompletion).IsAssignableFrom(typeof(AzureOpenAIChatCompletion)))
        {
            builder.WithAIService<ITextCompletion>(serviceId, Factory, setAsDefault);
        }

        return builder;
    }

    /// <summary>
    /// Adds the Azure OpenAI chat completion with data service to the list.
    /// More information: <see href="https://learn.microsoft.com/en-us/azure/ai-services/openai/use-your-data-quickstart"/>
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
    /// <param name="config">Required configuration for Azure OpenAI chat completion with data.</param>
    /// <param name="alsoAsTextCompletion">Whether to use the service also for text completion, if supported.</param>
    /// <param name="serviceId">A local identifier for the given AI service.</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAIChatCompletionService(this KernelBuilder builder,
        AzureOpenAIChatCompletionWithDataConfig config,
        bool alsoAsTextCompletion = true,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        AzureOpenAIChatCompletionWithData Factory(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory) => new(
            config,
            HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
            loggerFactory);

        builder.WithAIService<IChatCompletion>(serviceId, Factory, setAsDefault);

        if (alsoAsTextCompletion && typeof(ITextCompletion).IsAssignableFrom(typeof(AzureOpenAIChatCompletionWithData)))
        {
            builder.WithAIService<ITextCompletion>(serviceId, Factory, setAsDefault);
        }

        return builder;
    }

    /// <summary>
    /// Adds the OpenAI ChatGPT completion service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="modelId">OpenAI model name, see https://platform.openai.com/docs/models</param>
    /// <param name="apiKey">OpenAI API key, see https://platform.openai.com/account/api-keys</param>
    /// <param name="orgId">OpenAI organization id. This is usually optional unless your account belongs to multiple organizations.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="alsoAsTextCompletion">Whether to use the service also for text completion, if supported</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreOpenAIChatCompletionService(this KernelBuilder builder,
        string modelId,
        string endpoint,
        string apiKey,
        string? orgId = null,
        string? serviceId = null,
        bool alsoAsTextCompletion = true,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        OpenAIChatCompletion Factory(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory) => new(
            modelId, endpoint,
            apiKey,
            orgId,
            HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
            loggerFactory);

        builder.WithAIService<IChatCompletion>(serviceId, Factory, setAsDefault);

        // If the class implements the text completion interface, allow to use it also for semantic functions
        if (alsoAsTextCompletion && typeof(ITextCompletion).IsAssignableFrom(typeof(OpenAIChatCompletion)))
        {
            builder.WithAIService<ITextCompletion>(serviceId, Factory, setAsDefault);
        }

        return builder;
    }

    /// <summary>
    /// Adds the Azure OpenAI ChatGPT completion service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="openAIClient">Custom <see cref="OpenAIClient"/> for HTTP requests.</param>
    /// <param name="alsoAsTextCompletion">Whether to use the service also for text completion, if supported</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAIChatCompletionService(this KernelBuilder builder,
        string deploymentName,
        CoreOpenAIClient openAIClient,
        bool alsoAsTextCompletion = true,
        string? serviceId = null,
        string? modelId = null,
        bool setAsDefault = false)
    {
        AzureOpenAIChatCompletion Factory(ILoggerFactory loggerFactory)
        {
            return new(deploymentName, openAIClient, modelId, loggerFactory);
        };

        builder.WithAIService<IChatCompletion>(serviceId, Factory, setAsDefault);

        // If the class implements the text completion interface, allow to use it also for semantic functions
        if (alsoAsTextCompletion && typeof(ITextCompletion).IsAssignableFrom(typeof(AzureOpenAIChatCompletion)))
        {
            builder.WithAIService<ITextCompletion>(serviceId, Factory, setAsDefault);
        }

        return builder;
    }

    /// <summary>
    /// Adds the OpenAI ChatGPT completion service to the list.
    /// See https://platform.openai.com/docs for service details.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="openAIClient">Custom <see cref="OpenAIClient"/> for HTTP requests.</param>
    /// <param name="alsoAsTextCompletion">Whether to use the service also for text completion, if supported</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreOpenAIChatCompletionService(this KernelBuilder builder,
        string deploymentName,
        CoreOpenAIClient openAIClient,
        bool alsoAsTextCompletion = true,
        string? serviceId = null,
        bool setAsDefault = false)
    {
        OpenAIChatCompletion Factory(ILoggerFactory loggerFactory)
        {
            return new(deploymentName, openAIClient, loggerFactory);
        }

        builder.WithAIService<IChatCompletion>(serviceId, Factory, setAsDefault);

        // If the class implements the text completion interface, allow to use it also for semantic functions
        if (alsoAsTextCompletion && typeof(ITextCompletion).IsAssignableFrom(typeof(AzureOpenAIChatCompletion)))
        {
            builder.WithAIService<ITextCompletion>(serviceId, Factory, setAsDefault);
        }

        return builder;
    }

    #endregion

    #region Images

    /// <summary>
    /// Add the OpenAI DallE image generation service to the list
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="apiKey">OpenAI API key, see https://platform.openai.com/account/api-keys</param>
    /// <param name="orgId">OpenAI organization id. This is usually optional unless your account belongs to multiple organizations.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithOpenAIImageGenerationService(this KernelBuilder builder,
        string apiKey,
        string? orgId = null,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<IImageGeneration>(serviceId, (loggerFactory, httpHandlerFactory) =>
            new OpenAIImageGeneration(
                apiKey,
                orgId,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory),
            setAsDefault);

        return builder;
    }

    /// <summary>
    /// Add the  Azure OpenAI DallE image generation service to the list
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="setAsDefault">Whether the service should be the default for its type.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="maxRetryCount">Maximum number of attempts to retrieve the image generation operation result.</param>
    /// <returns>Self instance</returns>
    public static KernelBuilder WithCoreAzureOpenAIImageGenerationService(this KernelBuilder builder,
        string endpoint,
        string apiKey,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null,
        int maxRetryCount = 5)
    {
        builder.WithAIService<IImageGeneration>(serviceId, (loggerFactory, httpHandlerFactory) =>
            new AzureOpenAIImageGeneration(
                endpoint,
                apiKey,
                HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory),
                loggerFactory,
                maxRetryCount),
            setAsDefault);

        return builder;
    }

    #endregion

    private static CoreOpenAIClient CreateAzureOpenAIClient(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory, string deploymentName, string endpoint, AzureKeyCredential credentials, HttpClient? httpClient)
    {
        CoreOpenAIClientOptions options = CreateOpenAIClientOptions(loggerFactory, httpHandlerFactory, httpClient);

        return new(new Uri(endpoint), credentials, options);
    }

    private static CoreOpenAIClient CreateAzureOpenAIClient(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory, string deploymentName, string endpoint, TokenCredential credentials, HttpClient? httpClient)
    {
        CoreOpenAIClientOptions options = CreateOpenAIClientOptions(loggerFactory, httpHandlerFactory, httpClient);

        return new(new Uri(endpoint), credentials, options);
    }

    private static CoreOpenAIClientOptions CreateOpenAIClientOptions(ILoggerFactory loggerFactory, IDelegatingHandlerFactory httpHandlerFactory, HttpClient? httpClient)
    {
        CoreOpenAIClientOptions options = new()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            Transport = new HttpClientTransport(HttpClientProvider.GetHttpClient(httpHandlerFactory, httpClient, loggerFactory)),
            RetryPolicy = new RetryPolicy(maxRetries: 0) //Disabling Azure SDK retry policy to use the one provided by the delegating handler factory or the HTTP client.
        };
#pragma warning restore CA2000 // Dispose objects before losing scope

        return options;
    }
}

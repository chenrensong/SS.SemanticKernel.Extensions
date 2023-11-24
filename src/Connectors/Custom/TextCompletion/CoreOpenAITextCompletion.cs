﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.TextCompletion;

/// <summary>
/// OpenAI text completion service.
/// TODO: forward ETW logging to ILogger, see https://learn.microsoft.com/en-us/dotnet/azure/sdk/logging
/// </summary>
public sealed class CoreOpenAITextCompletion : OpenAIClientBase, ITextCompletion
{
    /// <summary>
    /// Create an instance of the OpenAI text completion connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="apiKey">OpenAI API Key</param>
    /// <param name="organization">OpenAI Organization Id (usually optional)</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    public CoreOpenAITextCompletion(
        string modelId,
        string endpoint,
        string apiKey,
        string? organization = null,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null
    ) : base(modelId, endpoint, apiKey, organization, httpClient, loggerFactory)
    {
        this.AddAttribute(IAIServiceExtensions.ModelIdKey, modelId);
        this.AddAttribute(OrganizationKey, organization!);
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> Attributes => this.InternalAttributes;

    /// <inheritdoc/>
    public IAsyncEnumerable<ITextStreamingResult> GetStreamingCompletionsAsync(
        string text,
        AIRequestSettings? requestSettings,
        CancellationToken cancellationToken = default)
    {
        this.LogActionDetails();
        return this.InternalGetTextStreamingResultsAsync(text, requestSettings, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ITextResult>> GetCompletionsAsync(
        string text,
        AIRequestSettings? requestSettings,
        CancellationToken cancellationToken = default)
    {
        this.LogActionDetails();
        return this.InternalGetTextResultsAsync(text, requestSettings, cancellationToken);
    }
}

﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;
/// <summary>
/// Base class for Azure OpenAI clients.
/// </summary>
public abstract class AzureOpenAIClientBase : ClientBase
{
    /// <summary>
    /// Key used to store the deployment name in the <see cref="IAIService.Attributes"/> dictionary.
    /// </summary>
    public const string DeploymentNameKey = "DeploymentName";

    /// <summary>
    /// OpenAI / Azure OpenAI Client
    /// </summary>
    private protected override CoreOpenAIClient Client { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureOpenAIClientBase"/> class using API Key authentication.
    /// </summary>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    private protected AzureOpenAIClientBase(
        string deploymentName,
        string endpoint,
        string apiKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null) : base(loggerFactory)
    {
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.StartsWith(endpoint, "https://", "The Azure OpenAI endpoint must start with 'https://'");
        Verify.NotNullOrWhiteSpace(apiKey);

        var options = GetClientOptions(httpClient);

        this.DeploymentOrModelName = deploymentName;
        this.Client = new CoreOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), options);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureOpenAIClientBase"/> class supporting AAD authentication.
    /// </summary>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credential">Token credential, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    private protected AzureOpenAIClientBase(
        string deploymentName,
        string endpoint,
        TokenCredential credential,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null) : base(loggerFactory)
    {
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.StartsWith(endpoint, "https://", "The Azure OpenAI endpoint must start with 'https://'");

        var options = GetClientOptions(httpClient);

        this.DeploymentOrModelName = deploymentName;
        this.Client = new CoreOpenAIClient(new Uri(endpoint), credential, options);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureOpenAIClientBase"/> class using the specified OpenAIClient.
    /// Note: instances created this way might not have the default diagnostics settings,
    /// it's up to the caller to configure the client.
    /// </summary>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="openAIClient">Custom <see cref="OpenAIClient"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    private protected AzureOpenAIClientBase(
        string deploymentName,
        CoreOpenAIClient openAIClient,
        ILoggerFactory? loggerFactory = null) : base(loggerFactory)
    {
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNull(openAIClient);

        this.DeploymentOrModelName = deploymentName;
        this.Client = openAIClient;

        this.AddAttribute(DeploymentNameKey, deploymentName);
    }

    /// <summary>
    /// Options used by the Azure OpenAI client, e.g. User Agent.
    /// </summary>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <returns>An instance of <see cref="OpenAIClientOptions"/>.</returns>
    private static CoreOpenAIClientOptions GetClientOptions(HttpClient? httpClient)
    {
        var options = new CoreOpenAIClientOptions
        {
            Diagnostics =
            {
                IsTelemetryEnabled = Telemetry.IsTelemetryEnabled,
                ApplicationId = Telemetry.HttpUserAgent,
            }
        };

        if (httpClient != null)
        {
            options.Transport = new HttpClientTransport(httpClient);
            options.RetryPolicy = new RetryPolicy(maxRetries: 0); //Disabling Azure SDK retry policy to use the one provided by the custom HTTP client.
        }

        return options;
    }

    /// <summary>
    /// Logs Azure OpenAI action details.
    /// </summary>
    /// <param name="callerMemberName">Caller member name. Populated automatically by runtime.</param>
    private protected void LogActionDetails([CallerMemberName] string? callerMemberName = default)
    {
        this.Logger.LogInformation("Action: {Action}. Azure OpenAI Deployment Name: {DeploymentName}.", callerMemberName, this.DeploymentOrModelName);
    }
}

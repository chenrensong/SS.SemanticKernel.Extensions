// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.AI.OpenAI
{
    // Data plane generated client.
    /// <summary> Azure OpenAI APIs for completions and search. </summary>
    public class CoreOpenAIClient 
    {
        private const int DefaultMaxCompletionsTokens = 100;

        private const string PublicOpenAIApiVersion = "1";

        private const string PublicOpenAIEndpoint = "https://api.openai.com/v1";

        private bool _isConfiguredForAzureOpenAI = true;

        private const string AuthorizationHeader = "api-key";

        private readonly AzureKeyCredential _keyCredential;

        private static readonly string[] AuthorizationScopes = new string[1] { "https://cognitiveservices.azure.com/.default" };

        private readonly TokenCredential _tokenCredential;

        private readonly HttpPipeline _pipeline;

        private readonly Uri _endpoint;

        private readonly string _apiVersion;

        private static RequestContext DefaultRequestContext = new RequestContext();

        private static ResponseClassifier _responseClassifier200;

        private static ResponseClassifier _responseClassifier202;

        internal Azure.Core.Pipeline.ClientDiagnostics ClientDiagnostics { get; }

        public virtual HttpPipeline Pipeline => _pipeline;

        private unsafe static ResponseClassifier ResponseClassifier200
        {
            get
            {
                ResponseClassifier responseClassifier = _responseClassifier200;
                if (responseClassifier == null)
                {
                    byte* intPtr = stackalloc byte[2];
                    *(short*)intPtr = 200;
                    responseClassifier = (_responseClassifier200 = new StatusCodeClassifier(new Span<ushort>(intPtr, 1)));
                }
                return responseClassifier;
            }
        }

        private unsafe static ResponseClassifier ResponseClassifier202
        {
            get
            {
                ResponseClassifier responseClassifier = _responseClassifier202;
                if (responseClassifier == null)
                {
                    byte* intPtr = stackalloc byte[2];
                    *(short*)intPtr = 202;
                    responseClassifier = (_responseClassifier202 = new StatusCodeClassifier(new Span<ushort>(intPtr, 1)));
                }
                return responseClassifier;
            }
        }

        public CoreOpenAIClient(Uri endpoint, AzureKeyCredential keyCredential, CoreOpenAIClientOptions options)
        {
            Azure.Core.Argument.AssertNotNull(endpoint, "endpoint");
            Azure.Core.Argument.AssertNotNull(keyCredential, "keyCredential");
            if (options == null)
            {
                options = new CoreOpenAIClientOptions();
            }
            ClientDiagnostics = new Azure.Core.Pipeline.ClientDiagnostics(options, true);
            _keyCredential = keyCredential;
            _pipeline = HttpPipelineBuilder.Build(options, Array.Empty<HttpPipelinePolicy>(), new HttpPipelinePolicy[1]
            {
            new AzureKeyCredentialPolicy(_keyCredential, "api-key")
            }, new ResponseClassifier());
            _endpoint = endpoint;
            _apiVersion = options.Version;
        }

        public CoreOpenAIClient(Uri endpoint, AzureKeyCredential keyCredential)
            : this(endpoint, keyCredential, new CoreOpenAIClientOptions())
        {
        }

        public CoreOpenAIClient(Uri endpoint, TokenCredential tokenCredential, CoreOpenAIClientOptions options)
        {
            Azure.Core.Argument.AssertNotNull(endpoint, "endpoint");
            Azure.Core.Argument.AssertNotNull(tokenCredential, "tokenCredential");
            if (options == null)
            {
                options = new CoreOpenAIClientOptions();
            }
            ClientDiagnostics = new Azure.Core.Pipeline.ClientDiagnostics(options, true);
            _tokenCredential = tokenCredential;
            _pipeline = HttpPipelineBuilder.Build(options, Array.Empty<HttpPipelinePolicy>(), new HttpPipelinePolicy[1]
            {
            new BearerTokenAuthenticationPolicy(_tokenCredential, AuthorizationScopes)
            }, new ResponseClassifier());
            _endpoint = endpoint;
            _apiVersion = options.Version;

            _isConfiguredForAzureOpenAI = false;
        }

        public CoreOpenAIClient(Uri endpoint, TokenCredential tokenCredential)
            : this(endpoint, tokenCredential, new CoreOpenAIClientOptions())
        {
        }

        public CoreOpenAIClient(string openAIApiKey, CoreOpenAIClientOptions options)
            : this(new Uri("https://api.openai.com/v1"), CreateDelegatedToken(openAIApiKey), options)
        {
            _isConfiguredForAzureOpenAI = false;
        }

        public CoreOpenAIClient(string openAIApiKey)
            : this(new Uri("https://api.openai.com/v1"), CreateDelegatedToken(openAIApiKey), new CoreOpenAIClientOptions())
        {
            _isConfiguredForAzureOpenAI = false;
        }

        public virtual Response<CoreCompletions> GetCompletions(string deploymentOrModelName, CoreCompletionsOptions completionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(completionsOptions, "completionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetCompletions"))
            {
                diagnosticScope.Start();
                completionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                completionsOptions.InternalShouldStreamResponse = null;
                RequestContent content = completionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    using (HttpMessage message = CreatePostRequestMessage(deploymentOrModelName, "completions", content, requestContext))
                    {
                        Response response = _pipeline.ProcessMessage(message, requestContext, cancellationToken);
                        return Response.FromValue(CoreCompletions.FromResponse(response), response);
                    }
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual Response<CoreCompletions> GetCompletions(string deploymentOrModelName, string prompt, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(prompt, "prompt");
            CoreCompletionsOptions defaultCompletionsOptions = GetDefaultCompletionsOptions(prompt);
            return GetCompletions(deploymentOrModelName, defaultCompletionsOptions, cancellationToken);
        }

        public virtual async Task<Response<CoreCompletions>> GetCompletionsAsync(string deploymentOrModelName, CoreCompletionsOptions completionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(completionsOptions, "completionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetCompletions"))
            {
                scope.Start();
                completionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                completionsOptions.InternalShouldStreamResponse = null;
                RequestContent content = completionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    using (HttpMessage message = CreatePostRequestMessage(deploymentOrModelName, "completions", content, requestContext))
                    {
                        Response response = await _pipeline.ProcessMessageAsync(message, requestContext, cancellationToken).ConfigureAwait(false);
                        return Response.FromValue(CoreCompletions.FromResponse(response), response);
                    }
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual Task<Response<CoreCompletions>> GetCompletionsAsync(string deploymentOrModelName, string prompt, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(prompt, "prompt");
            CoreCompletionsOptions defaultCompletionsOptions = GetDefaultCompletionsOptions(prompt);
            return GetCompletionsAsync(deploymentOrModelName, defaultCompletionsOptions, cancellationToken);
        }

        public virtual Response<CoreStreamingCompletions> GetCompletionsStreaming(string deploymentOrModelName, CoreCompletionsOptions completionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(completionsOptions, "completionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetCompletionsStreaming"))
            {
                diagnosticScope.Start();
                completionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                completionsOptions.InternalShouldStreamResponse = true;
                RequestContent content = completionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    HttpMessage httpMessage = CreatePostRequestMessage(deploymentOrModelName, "completions", content, requestContext);
                    httpMessage.BufferResponse = false;
                    Response response = _pipeline.ProcessMessage(httpMessage, requestContext, cancellationToken);
                    return Response.FromValue(new CoreStreamingCompletions(response), response);
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual async Task<Response<CoreStreamingCompletions>> GetCompletionsStreamingAsync(string deploymentOrModelName, CoreCompletionsOptions completionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(completionsOptions, "completionsOptions");
            completionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
            completionsOptions.InternalShouldStreamResponse = true;
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetCompletionsStreaming"))
            {
                scope.Start();
                RequestContent content = completionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    HttpMessage httpMessage = CreatePostRequestMessage(deploymentOrModelName, "completions", content, requestContext);
                    httpMessage.BufferResponse = false;
                    Response response = await _pipeline.ProcessMessageAsync(httpMessage, requestContext, cancellationToken).ConfigureAwait(false);
                    return Response.FromValue(new CoreStreamingCompletions(response), response);
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual Response<CoreChatCompletions> GetChatCompletions(string deploymentOrModelName, CoreChatCompletionsOptions chatCompletionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(chatCompletionsOptions, "chatCompletionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetChatCompletions"))
            {
                diagnosticScope.Start();
                chatCompletionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                chatCompletionsOptions.InternalShouldStreamResponse = null;
                string operationPath = GetOperationPath(chatCompletionsOptions);
                RequestContent content = chatCompletionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    using (HttpMessage message = CreatePostRequestMessage(deploymentOrModelName, operationPath, content, requestContext))
                    {
                        Response response = _pipeline.ProcessMessage(message, requestContext, cancellationToken);
                        return Response.FromValue(CoreChatCompletions.FromResponse(response), response);
                    }
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual async Task<Response<CoreChatCompletions>> GetChatCompletionsAsync(string deploymentOrModelName, CoreChatCompletionsOptions chatCompletionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(chatCompletionsOptions, "chatCompletionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetChatCompletions"))
            {
                scope.Start();
                chatCompletionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                chatCompletionsOptions.InternalShouldStreamResponse = null;
                string operationPath = GetOperationPath(chatCompletionsOptions);
                RequestContent content = chatCompletionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    using (HttpMessage message = CreatePostRequestMessage(deploymentOrModelName, operationPath, content, requestContext))
                    {
                        Response response = await _pipeline.ProcessMessageAsync(message, requestContext, cancellationToken).ConfigureAwait(false);
                        return Response.FromValue(CoreChatCompletions.FromResponse(response), response);
                    }
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual Response<StreamingChatCompletions> GetChatCompletionsStreaming(string deploymentOrModelName, CoreChatCompletionsOptions chatCompletionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(chatCompletionsOptions, "chatCompletionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetChatCompletionsStreaming"))
            {
                diagnosticScope.Start();
                chatCompletionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                chatCompletionsOptions.InternalShouldStreamResponse = true;
                string operationPath = GetOperationPath(chatCompletionsOptions);
                RequestContent content = chatCompletionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    HttpMessage httpMessage = CreatePostRequestMessage(deploymentOrModelName, operationPath, content, requestContext);
                    httpMessage.BufferResponse = false;
                    Response response = _pipeline.ProcessMessage(httpMessage, requestContext, cancellationToken);
                    return Response.FromValue(new StreamingChatCompletions(response), response);
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual async Task<Response<StreamingChatCompletions>> GetChatCompletionsStreamingAsync(string deploymentOrModelName, CoreChatCompletionsOptions chatCompletionsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(chatCompletionsOptions, "chatCompletionsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetChatCompletionsStreaming"))
            {
                scope.Start();
                chatCompletionsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                chatCompletionsOptions.InternalShouldStreamResponse = true;
                string operationPath = GetOperationPath(chatCompletionsOptions);
                RequestContent content = chatCompletionsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    HttpMessage httpMessage = CreatePostRequestMessage(deploymentOrModelName, operationPath, content, requestContext);
                    httpMessage.BufferResponse = false;
                    Response response = await _pipeline.ProcessMessageAsync(httpMessage, requestContext, cancellationToken).ConfigureAwait(false);
                    return Response.FromValue(new StreamingChatCompletions(response), response);
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual Response<CoreEmbeddings> GetEmbeddings(string deploymentOrModelName, CoreEmbeddingsOptions embeddingsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNullOrEmpty(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(embeddingsOptions, "embeddingsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetEmbeddings"))
            {
                diagnosticScope.Start();
                embeddingsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                RequestContent content = embeddingsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    HttpMessage message = CreatePostRequestMessage(deploymentOrModelName, "embeddings", content, requestContext);
                    Response response = _pipeline.ProcessMessage(message, requestContext, cancellationToken);
                    return Response.FromValue(CoreEmbeddings.FromResponse(response), response);
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual async Task<Response<CoreEmbeddings>> GetEmbeddingsAsync(string deploymentOrModelName, CoreEmbeddingsOptions embeddingsOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNullOrEmpty(deploymentOrModelName, "deploymentOrModelName");
            Azure.Core.Argument.AssertNotNull(embeddingsOptions, "embeddingsOptions");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetEmbeddings"))
            {
                scope.Start();
                embeddingsOptions.InternalNonAzureModelName = (_isConfiguredForAzureOpenAI ? null : deploymentOrModelName);
                RequestContent content = embeddingsOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                try
                {
                    HttpMessage message = CreatePostRequestMessage(deploymentOrModelName, "embeddings", content, requestContext);
                    Response response = await _pipeline.ProcessMessageAsync(message, requestContext, cancellationToken).ConfigureAwait(false);
                    return Response.FromValue(CoreEmbeddings.FromResponse(response), response);
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual Response<ImageGenerations> GetImageGenerations(CoreImageGenerationOptions imageGenerationOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(imageGenerationOptions, "imageGenerationOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetImageGenerations"))
            {
                diagnosticScope.Start();
                try
                {
                    Response response = null;
                    ImageGenerations imageGenerations = null;
                    if (_isConfiguredForAzureOpenAI)
                    {
                        Operation<CoreBatchImageGenerationOperationResponse> operation = BeginAzureBatchImageGeneration(WaitUntil.Completed, imageGenerationOptions, cancellationToken);
                        response = operation.GetRawResponse();
                        imageGenerations = operation.Value.Result;
                    }
                    else
                    {
                        RequestContext requestContext = FromCancellationToken(cancellationToken);
                        HttpMessage message = CreatePostRequestMessage(string.Empty, "images/generations", imageGenerationOptions.ToRequestContent(), requestContext);
                        response = _pipeline.ProcessMessage(message, requestContext, cancellationToken);
                        imageGenerations = ImageGenerations.FromResponse(response);
                    }
                    return Response.FromValue(imageGenerations, response);
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual async Task<Response<ImageGenerations>> GetImageGenerationsAsync(CoreImageGenerationOptions imageGenerationOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(imageGenerationOptions, "imageGenerationOptions");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetImageGenerations"))
            {
                scope.Start();
                try
                {
                    Response response;
                    ImageGenerations value;
                    if (_isConfiguredForAzureOpenAI)
                    {
                        Operation<CoreBatchImageGenerationOperationResponse> obj = await BeginAzureBatchImageGenerationAsync(WaitUntil.Completed, imageGenerationOptions, cancellationToken).ConfigureAwait(false);
                        response = obj.GetRawResponse();
                        value = obj.Value.Result;
                    }
                    else
                    {
                        RequestContext requestContext = FromCancellationToken(cancellationToken);
                        HttpMessage message = CreatePostRequestMessage(string.Empty, "images/generations", imageGenerationOptions.ToRequestContent(), requestContext);
                        response = await _pipeline.ProcessMessageAsync(message, requestContext, cancellationToken).ConfigureAwait(false);
                        value = ImageGenerations.FromResponse(response);
                    }
                    return Response.FromValue(value, response);
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        public virtual async Task<Response<CoreAudioTranscription>> GetAudioTranscriptionAsync(string deploymentId, CoreAudioTranscriptionOptions audioTranscriptionOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNullOrEmpty(deploymentId, "deploymentId");
            Azure.Core.Argument.AssertNotNull(audioTranscriptionOptions, "audioTranscriptionOptions");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetAudioTranscription"))
            {
                scope.Start();
                audioTranscriptionOptions.InternalNonAzureModelName = deploymentId;
                RequestContent content = audioTranscriptionOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                Response response = null;
                try
                {
                    using (HttpMessage message = CreateGetAudioTranscriptionRequest(deploymentId, content, requestContext))
                    {
                        response = await _pipeline.ProcessMessageAsync(message, requestContext).ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
                return Response.FromValue(CoreAudioTranscription.FromResponse(response), response);
            }
        }

        public virtual Response<CoreAudioTranscription> GetAudioTranscription(string deploymentId, CoreAudioTranscriptionOptions audioTranscriptionOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNullOrEmpty(deploymentId, "deploymentId");
            Azure.Core.Argument.AssertNotNull(audioTranscriptionOptions, "audioTranscriptionOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetAudioTranscription"))
            {
                diagnosticScope.Start();
                audioTranscriptionOptions.InternalNonAzureModelName = deploymentId;
                RequestContent content = audioTranscriptionOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                Response response = null;
                try
                {
                    using (HttpMessage message = CreateGetAudioTranscriptionRequest(deploymentId, content, requestContext))
                    {
                        response = _pipeline.ProcessMessage(message, requestContext);
                    }
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
                return Response.FromValue(CoreAudioTranscription.FromResponse(response), response);
            }
        }

        public virtual async Task<Response<CoreAudioTranslation>> GetAudioTranslationAsync(string deploymentId, CoreAudioTranslationOptions audioTranslationOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNullOrEmpty(deploymentId, "deploymentId");
            Azure.Core.Argument.AssertNotNull(audioTranslationOptions, "audioTranslationOptions");
            audioTranslationOptions.InternalNonAzureModelName = deploymentId;
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.GetAudioTranslation"))
            {
                scope.Start();
                audioTranslationOptions.InternalNonAzureModelName = deploymentId;
                RequestContent content = audioTranslationOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                Response response = null;
                try
                {
                    using (HttpMessage message = CreateGetAudioTranslationRequest(deploymentId, content, requestContext))
                    {
                        response = await _pipeline.ProcessMessageAsync(message, requestContext).ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
                return Response.FromValue(CoreAudioTranslation.FromResponse(response), response);
            }
        }

        public virtual Response<CoreAudioTranslation> GetAudioTranslation(string deploymentId, CoreAudioTranslationOptions audioTranslationOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNullOrEmpty(deploymentId, "deploymentId");
            Azure.Core.Argument.AssertNotNull(audioTranslationOptions, "audioTranslationOptions");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.GetAudioTranslation"))
            {
                diagnosticScope.Start();
                audioTranslationOptions.InternalNonAzureModelName = deploymentId;
                RequestContent content = audioTranslationOptions.ToRequestContent();
                RequestContext requestContext = FromCancellationToken(cancellationToken);
                Response response = null;
                try
                {
                    using (HttpMessage message = CreateGetAudioTranslationRequest(deploymentId, content, requestContext))
                    {
                        response = _pipeline.ProcessMessage(message, requestContext);
                    }
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
                return Response.FromValue(CoreAudioTranslation.FromResponse(response), response);
            }
        }

        internal RequestUriBuilder GetUri(string deploymentOrModelName, string operationPath)
        {
            RawRequestUriBuilder rawRequestUriBuilder = new RawRequestUriBuilder();
            rawRequestUriBuilder.Reset(_endpoint);
            if (_isConfiguredForAzureOpenAI)
            {
                rawRequestUriBuilder.AppendRaw("/openai", false);
                rawRequestUriBuilder.AppendPath("/deployments/", false);
                rawRequestUriBuilder.AppendPath(deploymentOrModelName, true);
                rawRequestUriBuilder.AppendPath("/" + operationPath, false);
                rawRequestUriBuilder.AppendQuery("api-version", _apiVersion, true);
            }
            else
            {
                rawRequestUriBuilder.AppendPath("/" + operationPath, false);
            }
            return rawRequestUriBuilder;
        }

        internal HttpMessage CreatePostRequestMessage(string deploymentOrModelName, string operationPath, RequestContent content, RequestContext context)
        {
            HttpMessage httpMessage = _pipeline.CreateMessage(context, ResponseClassifier200);
            Request request = httpMessage.Request;
            request.Method = RequestMethod.Post;
            request.Uri = GetUri(deploymentOrModelName, operationPath);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Content-Type", "application/json");
            request.Content = content;
            return httpMessage;
        }

        private static TokenCredential CreateDelegatedToken(string token)
        {
            AccessToken accessToken = new AccessToken(token, DateTimeOffset.Now.AddDays(180.0));
            return DelegatedTokenCredential.Create((TokenRequestContext _, CancellationToken _) => accessToken);
        }

        private static CoreCompletionsOptions GetDefaultCompletionsOptions(string prompt)
        {
            return new CoreCompletionsOptions
            {
                Prompts = { prompt },
                MaxTokens = 100
            };
        }

        private static string GetOperationPath(CoreChatCompletionsOptions chatCompletionsOptions)
        {
            if (chatCompletionsOptions.AzureExtensionsOptions == null)
            {
                return "chat/completions";
            }
            return "extensions/chat/completions";
        }

        internal HttpMessage CreateGetAudioTranscriptionRequest(string deploymentId, RequestContent content, RequestContext context)
        {
            HttpMessage httpMessage = _pipeline.CreateMessage(context, ResponseClassifier200);
            Request request = httpMessage.Request;
            request.Method = RequestMethod.Post;
            request.Uri = GetUri(deploymentId, "audio/transcriptions");
            request.Content = content;
            string boundary = (content as MultipartFormDataRequestContent).Boundary;
            request.Headers.Add("content-type", "multipart/form-data; boundary=" + boundary);
            return httpMessage;
        }

        internal HttpMessage CreateGetAudioTranslationRequest(string deploymentId, RequestContent content, RequestContext context)
        {
            HttpMessage httpMessage = _pipeline.CreateMessage(context, ResponseClassifier200);
            Request request = httpMessage.Request;
            request.Method = RequestMethod.Post;
            request.Uri = GetUri(deploymentId, "audio/translations");
            request.Content = content;
            string boundary = (content as MultipartFormDataRequestContent).Boundary;
            request.Headers.Add("content-type", "multipart/form-data; boundary=" + boundary);
            return httpMessage;
        }

        protected CoreOpenAIClient()
        {
        }

        internal virtual async Task<Operation<CoreBatchImageGenerationOperationResponse>> BeginAzureBatchImageGenerationAsync(WaitUntil waitUntil, CoreImageGenerationOptions imageGenerationOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(imageGenerationOptions, "imageGenerationOptions");
            RequestContext context = FromCancellationToken(cancellationToken);
            return ProtocolOperationHelpers.Convert(await BeginAzureBatchImageGenerationAsync(waitUntil, imageGenerationOptions.ToRequestContent(), context).ConfigureAwait(false), CoreBatchImageGenerationOperationResponse.FromResponse, ClientDiagnostics, "OpenAIClient.BeginAzureBatchImageGeneration");
        }

        internal virtual Operation<CoreBatchImageGenerationOperationResponse> BeginAzureBatchImageGeneration(WaitUntil waitUntil, CoreImageGenerationOptions imageGenerationOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            Azure.Core.Argument.AssertNotNull(imageGenerationOptions, "imageGenerationOptions");
            RequestContext context = FromCancellationToken(cancellationToken);
            return ProtocolOperationHelpers.Convert(BeginAzureBatchImageGeneration(waitUntil, imageGenerationOptions.ToRequestContent(), context), CoreBatchImageGenerationOperationResponse.FromResponse, ClientDiagnostics, "OpenAIClient.BeginAzureBatchImageGeneration");
        }

        internal virtual async Task<Operation<BinaryData>> BeginAzureBatchImageGenerationAsync(WaitUntil waitUntil, RequestContent content, RequestContext context = null)
        {
            Azure.Core.Argument.AssertNotNull(content, "content");
            using (Azure.Core.Pipeline.DiagnosticScope scope = ClientDiagnostics.CreateScope("OpenAIClient.BeginAzureBatchImageGeneration"))
            {
                scope.Start();
                try
                {
                    using (HttpMessage message = CreateBeginAzureBatchImageGenerationRequest(content, context))
                    {
                        return await ProtocolOperationHelpers.ProcessMessageAsync(_pipeline, message, ClientDiagnostics, "OpenAIClient.BeginAzureBatchImageGeneration", OperationFinalStateVia.Location, context, waitUntil).ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    scope.Failed(exception);
                    throw;
                }
            }
        }

        internal virtual Operation<BinaryData> BeginAzureBatchImageGeneration(WaitUntil waitUntil, RequestContent content, RequestContext context = null)
        {
            Azure.Core.Argument.AssertNotNull(content, "content");
            using (Azure.Core.Pipeline.DiagnosticScope diagnosticScope = ClientDiagnostics.CreateScope("OpenAIClient.BeginAzureBatchImageGeneration"))
            {
                diagnosticScope.Start();
                try
                {
                    using (HttpMessage message = CreateBeginAzureBatchImageGenerationRequest(content, context))
                    {
                        return ProtocolOperationHelpers.ProcessMessage(_pipeline, message, ClientDiagnostics, "OpenAIClient.BeginAzureBatchImageGeneration", OperationFinalStateVia.Location, context, waitUntil);
                    }
                }
                catch (Exception exception)
                {
                    diagnosticScope.Failed(exception);
                    throw;
                }
            }
        }

        internal HttpMessage CreateBeginAzureBatchImageGenerationRequest(RequestContent content, RequestContext context)
        {
            HttpMessage httpMessage = _pipeline.CreateMessage(context, ResponseClassifier202);
            Request request = httpMessage.Request;
            request.Method = RequestMethod.Post;
            RawRequestUriBuilder rawRequestUriBuilder = new RawRequestUriBuilder();
            rawRequestUriBuilder.Reset(_endpoint);
            rawRequestUriBuilder.AppendRaw("/openai", false);
            rawRequestUriBuilder.AppendPath("/images/generations:submit", false);
            rawRequestUriBuilder.AppendQuery("api-version", _apiVersion, true);
            request.Uri = rawRequestUriBuilder;
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Content-Type", "application/json");
            request.Content = content;
            return httpMessage;
        }

        internal static RequestContext FromCancellationToken(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return DefaultRequestContext;
            }
            return new RequestContext
            {
                CancellationToken = cancellationToken
            };
        }
    }

}

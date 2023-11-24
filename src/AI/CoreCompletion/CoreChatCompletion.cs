using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Services;
using SS.SemanticKernel.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.SemanticKernel.CreateChatCompletionRequest;

namespace Microsoft.SemanticKernel {
    public sealed class CoreChatCompletion : IChatCompletion, ITextCompletion
    {
        private const int MaxResultsPerPrompt = 128;

        public CoreChatCompletion(string modelId, string endpoint, string key, HttpClient? httpClient = null, ILogger? logger = null)
        {
            _modelId = modelId;
            _key = key;
            _endpoint = endpoint;
            _logger = logger;
            _httpClient = httpClient;
        }

        private string _modelId;

        private string _key;

        private string _endpoint;

        private HttpClient? _httpClient;

        private ILogger? _logger;

        /// <summary>
        /// Create an instance of the <see cref="AzureOpenAIChatCompletion"/> connector with API key auth.
        /// </summary>
        /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
        /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
        /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
        /// <param name="modelId">Azure OpenAI model id, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
        /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
        public AzureOpenAIChatCompletion(
            string deploymentName,
            string endpoint,
            string apiKey,
            string? modelId = null,
            HttpClient? httpClient = null,
            ILoggerFactory? loggerFactory = null) : base(deploymentName, endpoint, apiKey, httpClient, loggerFactory)
        {
            this.AddAttribute(IAIServiceExtensions.ModelIdKey, modelId);
        }

        /// <summary>
        /// Create an instance of the <see cref="AzureOpenAIChatCompletion"/> connector with AAD auth.
        /// </summary>
        /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
        /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
        /// <param name="credentials">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
        /// <param name="modelId">Azure OpenAI model id, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
        /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
        public AzureOpenAIChatCompletion(
            string deploymentName,
            string endpoint,
            TokenCredential credentials,
            string? modelId = null,
            HttpClient? httpClient = null,
            ILoggerFactory? loggerFactory = null) : base(deploymentName, endpoint, credentials, httpClient, loggerFactory)
        {
            this.AddAttribute(IAIServiceExtensions.ModelIdKey, modelId);
        }

        /// <summary>
        /// Creates a new <see cref="AzureOpenAIChatCompletion"/> client instance using the specified <see cref="OpenAIClient"/>.
        /// </summary>
        /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
        /// <param name="openAIClient">Custom <see cref="OpenAIClient"/>.</param>
        /// <param name="modelId">Azure OpenAI model id, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
        public AzureOpenAIChatCompletion(
            string deploymentName,
            OpenAIClient openAIClient,
            string? modelId = null,
            ILoggerFactory? loggerFactory = null) : base(deploymentName, openAIClient, loggerFactory)
        {
            this.AddAttribute(IAIServiceExtensions.ModelIdKey, modelId);
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Attributes => this.InternalAttributes;

        /// <inheritdoc/>
        public Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(
            ChatHistory chat,
            AIRequestSettings? requestSettings = null,
            CancellationToken cancellationToken = default)
        {
            this.LogActionDetails();
            return this.InternalGetChatResultsAsync(chat, requestSettings, cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IChatStreamingResult> GetStreamingChatCompletionsAsync(
            ChatHistory chat,
            AIRequestSettings? requestSettings = null,
            CancellationToken cancellationToken = default)
        {
            this.LogActionDetails();
            return this.InternalGetChatStreamingResultsAsync(chat, requestSettings, cancellationToken);
        }

        /// <inheritdoc/>
        public ChatHistory CreateNewChat(string? instructions = null)
        {
            return InternalCreateNewChat(instructions);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<ITextStreamingResult> GetStreamingCompletionsAsync(
            string text,
            AIRequestSettings? requestSettings = null,
            CancellationToken cancellationToken = default)
        {
            this.LogActionDetails();
            return this.InternalGetChatStreamingResultsAsTextAsync(text, requestSettings, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<ITextResult>> GetCompletionsAsync(
            string text,
            AIRequestSettings? requestSettings = null,
            CancellationToken cancellationToken = default)
        {
            this.LogActionDetails();
            return this.InternalGetChatResultsAsTextAsync(text, requestSettings, cancellationToken);
        }





        private async IAsyncEnumerable<ITextStreamingResult> InternalGetChatStreamingResultsAsTextAsync(
                string text,
                CompleteRequestSettings? textSettings,
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ChatHistory chat = PrepareChatHistory(text, textSettings, out ChatRequestSettings chatSettings);

            await foreach (var chatCompletionStreamingResult in this.InternalGetChatStreamingResultsAsync(chat, chatSettings, cancellationToken))
            {
                yield return (ITextStreamingResult)chatCompletionStreamingResult;
            }
        }


        private async IAsyncEnumerable<TResponse> GetChatCompletionsStreamingAsync<TRequest, TResponse>(string requestUri, TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(_key))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");
            }
            using var httpResponseMessage = await _httpClient.PostAsStream(requestUri, request!, cancellationToken);
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            while (streamReader.EndOfStream is false)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var line = await streamReader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var dataPosition = line.IndexOf("data: ", StringComparison.Ordinal);
                line = dataPosition != 0 ? line : line.Substring("data: ".Length);

                if (line.StartsWith("[DONE]"))
                {
                    break;
                }

                TResponse? createCompletionResponse = default;

                try
                {
                    createCompletionResponse = JsonSerializer.Deserialize<TResponse>(line);
                }
                catch (Exception)
                {
                }

                if (createCompletionResponse is not null)
                {
                    yield return createCompletionResponse;
                }
            }
        }


        private static ChatHistory InternalCreateNewChat(string? instructions = null)
        {
            return new CoreChatHistory(instructions);
        }


        private async Task<IReadOnlyList<ITextResult>> InternalGetChatResultsAsTextAsync(
                    string text,
                    CompleteRequestSettings? textSettings,
                    CancellationToken cancellationToken = default)
        {
            textSettings ??= new();
            ChatHistory chat = PrepareChatHistory(text, textSettings, out ChatRequestSettings chatSettings);

            return (await this.InternalGetChatResultsAsync(chat, chatSettings, cancellationToken).ConfigureAwait(false))
                .OfType<ITextResult>()
                .ToList();
        }

        private static ChatHistory PrepareChatHistory(string text, CompleteRequestSettings? requestSettings, out ChatRequestSettings settings)
        {
            requestSettings ??= new();
            var chat = InternalCreateNewChat();
            chat.AddUserMessage(text);
            settings = new ChatRequestSettings
            {
                MaxTokens = requestSettings.MaxTokens,
                Temperature = requestSettings.Temperature,
                TopP = requestSettings.TopP,
                PresencePenalty = requestSettings.PresencePenalty,
                FrequencyPenalty = requestSettings.FrequencyPenalty,
                StopSequences = requestSettings.StopSequences,
            };
            return chat;
        }

  


        private async IAsyncEnumerable<IChatStreamingResult> InternalGetChatStreamingResultsAsync(
             IEnumerable<ChatMessageBase> chat,
             ChatRequestSettings? requestSettings,
             [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            requestSettings ??= new();
            ValidateMaxTokens(requestSettings.MaxTokens);
            var options = CreateChatCompletionsOptions(requestSettings, chat);
            await foreach (var completion in GetChatCompletionsStreamingAsync<CreateChatCompletionRequest, CreateChatCompletionResponse>(_endpoint,
                options, cancellationToken))
            {
                yield return new CoreChatStreamingResult(completion, completion.Choices);
            }

        }

        /// <summary>
        /// Generate a new chat message
        /// </summary>
        /// <param name="chat">Chat history</param>
        /// <param name="chatSettings">AI request settings</param>
        /// <param name="cancellationToken">Async cancellation token</param>
        /// <returns>Generated chat message in string format</returns>
        private async Task<IReadOnlyList<IChatResult>> InternalGetChatResultsAsync(
            ChatHistory chat,
            ChatRequestSettings? chatSettings,
            CancellationToken cancellationToken = default)
        {
            chatSettings ??= new();
            ValidateMaxTokens(chatSettings.MaxTokens);
            var chatOptions = CreateChatCompletionsOptions(chatSettings, chat);
            if (!string.IsNullOrEmpty(_key))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");
            }
            var response = await _httpClient.Post<CreateChatCompletionResponse>(_endpoint, chatOptions, cancellationToken).ConfigureAwait(false);
            if (response == null)
            {
                throw new CoreInvalidResponseException<CreateChatCompletionResponse>(null, "Chat completions null response");
            }
            if (response.Choices.Count == 0)
            {
                throw new CoreInvalidResponseException<CreateChatCompletionResponse>(response, "Chat completions not found");
            }
            return response.Choices.Select(chatChoice => new CoreChatResult(response, chatChoice)).ToList();
        }


        private static CreateChatCompletionRequest CreateChatCompletionsOptions(ChatRequestSettings requestSettings, IEnumerable<ChatMessageBase> chatHistory)
        {
            if (requestSettings.ResultsPerPrompt is < 1 or > MaxResultsPerPrompt)
            {
                throw new ArgumentOutOfRangeException($"{nameof(requestSettings)}.{nameof(requestSettings.ResultsPerPrompt)}", requestSettings.ResultsPerPrompt, $"The value must be in range between 1 and {MaxResultsPerPrompt}, inclusive.");
            }

            var options = new CreateChatCompletionRequest
            {
                MaxTokens = requestSettings.MaxTokens,
                Temperature = (float?)requestSettings.Temperature,
                TopP = (float?)requestSettings.TopP,
                FrequencyPenalty = (float?)requestSettings.FrequencyPenalty,
                PresencePenalty = (float?)requestSettings.PresencePenalty,
                N = requestSettings.ResultsPerPrompt
            };

            if (requestSettings.StopSequences is { Count: > 0 })
            {
                if (options.Stop == null)
                {
                    options.Stop = new List<string>();
                }

                foreach (var s in requestSettings.StopSequences)
                {
                    options.Stop.Add(s);
                }
            }

            foreach (var message in chatHistory)
            {
                var validRole = GetValidChatRole(message.Role);
                options.Messages.Add(new ChatCompletionMessage(validRole.Label, message.Content));
            }

            return options;
        }

        private static ChatRole GetValidChatRole(AuthorRole role)
        {
            var validRole = new ChatRole(role.Label);

            if (validRole != ChatRole.User &&
                validRole != ChatRole.System &&
                validRole != ChatRole.Assistant)
            {
                throw new ArgumentException($"Invalid chat message author role: {role}");
            }

            return validRole;
        }

        private static void ValidateMaxTokens(int maxTokens)
        {
            if (maxTokens < 1)
            {
                throw new AIException(
                    AIException.ErrorCodes.InvalidRequest,
                    $"MaxTokens {maxTokens} is not valid, the value must be greater than zero");
            }
        }

 
    }
}


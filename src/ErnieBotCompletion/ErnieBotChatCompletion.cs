﻿using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
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
using static Microsoft.SemanticKernel.ErnieBotChatResult;

namespace Microsoft.SemanticKernel
{
    public sealed class ErnieBotChatCompletion : IChatCompletion, ITextCompletion
    {
        private const int MaxResultsPerPrompt = 128;

        public ErnieBotChatCompletion(string modelId, string endpoint, string key, HttpClient? httpClient = null, ILogger? logger = null)
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

        public ChatHistory CreateNewChat(string instructions = null)
        {
            return InternalCreateNewChat(instructions);
        }

        public Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(ChatHistory chat, ChatRequestSettings requestSettings = null, CancellationToken cancellationToken = default)
        {
            return this.InternalGetChatResultsAsync(chat, requestSettings, cancellationToken);
        }

        public Task<IReadOnlyList<ITextCompletionResult>> GetCompletionsAsync(string text, CompleteRequestSettings requestSettings, CancellationToken cancellationToken = default)
        {
            return this.InternalGetChatResultsAsTextAsync(text, requestSettings, cancellationToken);
        }

        public IAsyncEnumerable<IChatStreamingResult> GetStreamingChatCompletionsAsync(ChatHistory chat, ChatRequestSettings requestSettings = null, CancellationToken cancellationToken = default)
        {
            return this.InternalGetChatStreamingResultsAsync(chat, requestSettings, cancellationToken);
        }

        public IAsyncEnumerable<ITextCompletionStreamingResult> GetStreamingCompletionsAsync(string text, CompleteRequestSettings requestSettings, CancellationToken cancellationToken = default)
        {
            return this.InternalGetChatStreamingResultsAsTextAsync(text, requestSettings, cancellationToken);
        }

        private async IAsyncEnumerable<ITextCompletionStreamingResult> InternalGetChatStreamingResultsAsTextAsync(
                string text,
                CompleteRequestSettings? textSettings,
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ChatHistory chat = PrepareChatHistory(text, textSettings, out ChatRequestSettings chatSettings);

            await foreach (var chatCompletionStreamingResult in this.InternalGetChatStreamingResultsAsync(chat, chatSettings, cancellationToken))
            {
                yield return (ITextCompletionStreamingResult)chatCompletionStreamingResult;
            }
        }


        private async IAsyncEnumerable<TResponse> GetChatCompletionsStreamingAsync<TRequest, TResponse>(string requestUri, TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var uri = $"{requestUri}?access_token={_key}";
            using var httpResponseMessage = await _httpClient.PostAsStream(uri, request!, cancellationToken);
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


        private async Task<IReadOnlyList<ITextCompletionResult>> InternalGetChatResultsAsTextAsync(
                    string text,
                    CompleteRequestSettings? textSettings,
                    CancellationToken cancellationToken = default)
        {
            textSettings ??= new();
            ChatHistory chat = PrepareChatHistory(text, textSettings, out ChatRequestSettings chatSettings);

            return (await this.InternalGetChatResultsAsync(chat, chatSettings, cancellationToken).ConfigureAwait(false))
                .OfType<ITextCompletionResult>()
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
            await foreach (var completion in GetChatCompletionsStreamingAsync<ErnieBotCompletionRequest, ErnieBotCompletionResponse>(_endpoint,
                options, cancellationToken))
            {
                yield return new ErnieBotChatStreamingResult(completion, options.Messages.LastOrDefault());
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
            var uri = $"{_endpoint}?access_token={_key}";
            var response = await _httpClient.Post<ErnieBotCompletionResponse>(uri, chatOptions, cancellationToken).ConfigureAwait(false);
            if (response == null)
            {
                throw new CoreInvalidResponseException<ErnieBotCompletionResponse>(null, "Chat completions null response");
            }
            if (response.Result == null)
            {
                throw new CoreInvalidResponseException<ErnieBotCompletionResponse>(response, "Chat completions not found");
            }
            var list = new List<ErnieBotChatResult> { new ErnieBotChatResult(response, chatOptions.Messages.LastOrDefault()) };
            return list;
        }


        private static ErnieBotCompletionRequest CreateChatCompletionsOptions(ChatRequestSettings requestSettings, IEnumerable<ChatMessageBase> chatHistory)
        {
            if (requestSettings.ResultsPerPrompt is < 1 or > MaxResultsPerPrompt)
            {
                throw new ArgumentOutOfRangeException($"{nameof(requestSettings)}.{nameof(requestSettings.ResultsPerPrompt)}", requestSettings.ResultsPerPrompt, $"The value must be in range between 1 and {MaxResultsPerPrompt}, inclusive.");
            }

            var options = new ErnieBotCompletionRequest
            {
            };

            foreach (var message in chatHistory)
            {
                var validRole = GetValidChatRole(message.Role);
                options.Messages.Add(new ErnieBotMessage(validRole.Label, message.Content));
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


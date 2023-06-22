using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using static Microsoft.SemanticKernel.AI.ChatCompletion.ChatHistory;
using static Microsoft.SemanticKernel.CreateChatCompletionRequest;
using Azure;

namespace Microsoft.SemanticKernel
{
    public class ErnieBotCompletionRequest
    {
        [JsonProperty("messages")]
        [JsonPropertyName("messages ")]
        public List<ErnieBotMessage> Messages { get; set; } = new List<ErnieBotMessage>();

        [JsonProperty("stream")]
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonProperty("user_id")]
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
    }

    public class ErnieBotCompletionResponse
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("error_code")]
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_msg")]
        [JsonPropertyName("error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("object")]
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonProperty("result")]
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonProperty("need_clear_history")]
        [JsonPropertyName("need_clear_history")]
        public bool NeedClearHistory { get; set; }

        [JsonProperty("usage")]
        [JsonPropertyName("usage")]
        public ErnieBotUsage Usage { get; set; }
    }

    public class ErnieBotUsage
    {
        [JsonProperty("prompt_tokens")]
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }


    public class Usage
    {

        [JsonProperty("prompt_tokens")]
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }


        [JsonProperty("completion_tokens")]
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }


        [JsonProperty("total_tokens")]
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class ErnieBotMessage
    {

        public ErnieBotMessage(string role, string content)
        {
            Role = role.ToLower();
            Content = content;
        }


        [JsonProperty("role")]
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class ErnieBotChatMessage : ChatMessageBase
    {
        /// <summary>
        /// Create a new instance of a chat message
        /// </summary>
        /// <param name="message">OpenAI SDK chat message representation</param>
        public ErnieBotChatMessage(ErnieBotMessage message)
            : base(new AuthorRole(message.Role.ToString()), message.Content)
        {
        }
    }

    internal sealed class ErnieBotChatResult : IChatResult, ITextCompletionResult
    {
        private readonly ModelResult _modelResult;
        private readonly ErnieBotCompletionResponse _resultData;
        private readonly ErnieBotMessage _message;
        public ErnieBotChatResult(ErnieBotCompletionResponse resultData, ErnieBotMessage message)
        {
            this._resultData = resultData;
            this._message = message;
            this._modelResult = new ModelResult(resultData);
        }

        public ModelResult ModelResult => this._modelResult;

        public Task<ChatMessageBase> GetChatMessageAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ChatMessageBase>(new ErnieBotChatMessage(_message));
        }


        public Task<string> GetCompletionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this._resultData.Result);
        }


        internal sealed class ErnieBotChatStreamingResult : IChatStreamingResult, ITextCompletionStreamingResult
        {
            private readonly ModelResult _modelResult;
            private readonly ErnieBotMessage _message;
            private readonly ErnieBotCompletionResponse _resultData;

            public ErnieBotChatStreamingResult(ErnieBotCompletionResponse resultData, ErnieBotMessage message)
            {
                this._resultData = resultData;
                this._modelResult = new ModelResult(resultData);
                this._message = message;
            }

            public ModelResult ModelResult => this._modelResult;

            /// <inheritdoc/>
            public Task<ChatMessageBase> GetChatMessageAsync(CancellationToken cancellationToken = default)
            {
                var chatMessage = this._message;
                if (chatMessage is null)
                {
                    throw new AIException(AIException.ErrorCodes.UnknownError, "Unable to get chat message from stream");
                }
                return Task.FromResult<ChatMessageBase>(new ErnieBotChatMessage(chatMessage));
            }

            /// <inheritdoc/>
            public async IAsyncEnumerable<ChatMessageBase> GetStreamingChatMessageAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await Task.Yield();
                var list = new List<ErnieBotChatMessage> { new ErnieBotChatMessage(_message) };
                foreach (var c in list)
                {
                    yield return c;
                }
            }

            /// <inheritdoc/>
            public async Task<string> GetCompletionAsync(CancellationToken cancellationToken = default)
            {
                return (await this.GetChatMessageAsync(cancellationToken).ConfigureAwait(false)).Content;
            }

            /// <inheritdoc/>
            public async IAsyncEnumerable<string> GetCompletionStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await foreach (var result in this.GetStreamingChatMessageAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield return result.Content;
                }
            }
        }
    }
}

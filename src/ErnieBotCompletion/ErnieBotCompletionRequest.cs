using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel {
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

    internal sealed class ErnieBotChatResult : IChatResult, ITextResult
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


        internal sealed class ErnieBotChatStreamingResult : IChatStreamingResult, ITextStreamingResult
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

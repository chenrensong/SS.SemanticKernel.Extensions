﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Orchestration;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.ChatCompletionWithData;

internal sealed class ChatWithDataStreamingResult : IChatStreamingResult, ITextStreamingResult, IChatResult, ITextResult
{
    public ModelResult ModelResult { get; }

    public ChatWithDataStreamingResult(ChatWithDataStreamingResponse response, ChatWithDataStreamingChoice choice)
    {
        Verify.NotNull(response);
        Verify.NotNull(choice);

        this.ModelResult = new(new ChatWithDataModelResult(response.Id, DateTimeOffset.FromUnixTimeSeconds(response.Created))
        {
            ToolContent = this.GetToolContent(choice)
        });

        this._choice = choice;
    }

    public async Task<ChatMessage> GetChatMessageAsync(CancellationToken cancellationToken = default)
    {
        var message = this._choice.Messages.FirstOrDefault(this.IsValidMessage);

        var result = new AzureOpenAIChatMessage(AuthorRole.Assistant.Label, message?.Delta?.Content ?? string.Empty);

        return await Task.FromResult<ChatMessage>(result).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<ChatMessage> GetStreamingChatMessageAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var message = await this.GetChatMessageAsync(cancellationToken).ConfigureAwait(false);

        if (message.Content is { Length: > 0 })
        {
            yield return message;
        }
    }

    public async IAsyncEnumerable<string> GetCompletionStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var result in this.GetStreamingChatMessageAsync(cancellationToken))
        {
            if (result.Content is string content and { Length: > 0 })
            {
                yield return content;
            }
        }
    }

    public async Task<string> GetCompletionAsync(CancellationToken cancellationToken = default)
    {
        var message = await this.GetChatMessageAsync(cancellationToken).ConfigureAwait(false);

        return message.Content;
    }

    #region private ================================================================================

    private readonly ChatWithDataStreamingChoice _choice;

    private bool IsValidMessage(ChatWithDataStreamingMessage message)
    {
        return !message.EndTurn &&
            (message.Delta.Role is null || !message.Delta.Role.Equals(AuthorRole.Tool.Label, StringComparison.Ordinal));
    }

    private string? GetToolContent(ChatWithDataStreamingChoice choice)
    {
        var message = choice.Messages
            .FirstOrDefault(message => message.Delta.Role is not null && message.Delta.Role.Equals(AuthorRole.Tool.Label, StringComparison.Ordinal));

        return message?.Delta?.Content;
    }

    #endregion
}

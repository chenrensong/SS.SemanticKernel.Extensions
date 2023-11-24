﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;

/// <summary>
/// Provides extension methods for the IChatResult interface.
/// </summary>
public static class ChatResultExtensions
{
    /// <summary>
    /// Retrieve the resulting function from the chat result.
    /// </summary>
    /// <param name="chatResult"></param>
    /// <returns>The <see cref="OpenAIFunctionResponse"/>, or null if no function was returned by the model.</returns>
    [Obsolete("Obsoleted, please use GetOpenAIFunctionResponse instead")]
    public static OpenAIFunctionResponse? GetFunctionResponse(this IChatResult chatResult)
    {
        return GetOpenAIFunctionResponse(chatResult);
    }

    /// <summary>
    /// Retrieve the resulting function from the chat result.
    /// </summary>
    /// <param name="chatResult"></param>
    /// <returns>The <see cref="OpenAIFunctionResponse"/>, or null if no function was returned by the model.</returns>
    public static OpenAIFunctionResponse? GetOpenAIFunctionResponse(this IChatResult chatResult)
    {
        OpenAIFunctionResponse? functionResponse = null;
        var functionCall = chatResult.ModelResult.GetResult<ChatModelResult>().Choice.Message.FunctionCall;
        if (functionCall is not null)
        {
            functionResponse = OpenAIFunctionResponse.FromFunctionCall(functionCall);
        }
        return functionResponse;
    }
}

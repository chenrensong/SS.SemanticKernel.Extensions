﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Azure.AI.OpenAI;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;

/// <summary>
/// Object containing function information and parameter values for a function call generated by the OpenAI model.
/// </summary>
public class OpenAIFunctionResponse
{
    /// <summary>
    /// Name of the function chosen
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the function's associated plugin, if applicable
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// Parameter values
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Fully qualified name of the function. This is the concatenation of the plugin name and the function name,
    /// separated by the value of <see cref="OpenAIFunction.NameSeparator"/>.
    /// If there is no plugin name, this is the same as the function name.
    /// </summary>
    public string FullyQualifiedName =>
        string.IsNullOrEmpty(this.PluginName) ? this.FunctionName : $"{this.PluginName}{OpenAIFunction.NameSeparator}{this.FunctionName}";

    /// <summary>
    /// Parses the function call and parameter information generated by the model.
    /// </summary>
    /// <param name="functionCall">The OpenAI function call object generated by the model.</param>
    /// <returns>Instance of <see cref="OpenAIFunctionResponse"/>.</returns>
    public static OpenAIFunctionResponse FromFunctionCall(CoreFunctionCall functionCall)
    {
        OpenAIFunctionResponse response = new();
        if (functionCall.Name.Contains(OpenAIFunction.NameSeparator))
        {
            var parts = functionCall.Name.Split(new string[] { OpenAIFunction.NameSeparator }, StringSplitOptions.RemoveEmptyEntries);
            response.PluginName = parts[0];
            response.FunctionName = parts[1];
        }
        else
        {
            response.FunctionName = functionCall.Name;
        }

        var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(functionCall.Arguments);
        if (parameters is not null)
        {
            response.Parameters = parameters;
        }

        return response;
    }
}

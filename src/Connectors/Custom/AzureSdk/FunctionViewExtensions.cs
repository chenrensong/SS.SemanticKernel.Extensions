﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;

/// <summary>
/// Extensions for <see cref="FunctionView"/> specific to the OpenAI connector.
/// </summary>
public static class FunctionViewExtensions
{
    /// <summary>
    /// Convert a <see cref="FunctionView"/> to an <see cref="OpenAIFunction"/>.
    /// </summary>
    /// <param name="functionView">The <see cref="FunctionView"/> object to convert.</param>
    /// <returns>An <see cref="OpenAIFunction"/> object.</returns>
    public static OpenAIFunction ToOpenAIFunction(this FunctionView functionView)
    {
        var openAIParams = new List<OpenAIFunctionParameter>();
        foreach (ParameterView param in functionView.Parameters)
        {
            openAIParams.Add(new OpenAIFunctionParameter
            {
                Name = param.Name,
                Description = (param.Description ?? string.Empty)
                    + (string.IsNullOrEmpty(param.DefaultValue) ? string.Empty : $" (default value: {param.DefaultValue})"),
                Type = param.Type?.Name ?? "string",
                IsRequired = param.IsRequired ?? false,
                ParameterType = param.ParameterType,
                Schema = param.Schema ?? OpenAIFunction.GetJsonSchemaDocument(param.ParameterType!, param.Description),
            });
        }

        var returnParameter = new OpenAIFunctionReturnParameter
        {
            Description = functionView.ReturnParameter.Description ?? string.Empty,
            Schema = functionView.ReturnParameter.Schema ?? OpenAIFunction.GetJsonSchemaDocument(functionView.ReturnParameter.ParameterType, functionView.ReturnParameter.Description),
        };

        return new OpenAIFunction
        {
            FunctionName = functionView.Name,
            PluginName = functionView.PluginName,
            Description = functionView.Description,
            Parameters = openAIParams,
            ReturnParameter = returnParameter
        };
    }
}

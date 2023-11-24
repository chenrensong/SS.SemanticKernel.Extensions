// Copyright (c) Microsoft. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace Microsoft.SemanticKernel.Connectors.AI.Custom.AzureSdk;

internal sealed class TextResult : ITextResult
{
    private readonly ModelResult _modelResult;
    private readonly CoreChoice _choice;

    public TextResult(CoreCompletions resultData, CoreChoice choice)
    {
        this._modelResult = new(new TextModelResult(resultData, choice));
        this._choice = choice;
    }

    public ModelResult ModelResult => this._modelResult;

    public Task<string> GetCompletionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this._choice.Text);
    }
}

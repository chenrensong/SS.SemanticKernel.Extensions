// Azure.AI.OpenAI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=92742159e12e44c8
// Azure.Core.AzureKeyCredentialPolicy
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;

internal class AzureKeyCredentialPolicy : HttpPipelineSynchronousPolicy
{
    private readonly string _name;

    private readonly AzureKeyCredential _credential;

    public AzureKeyCredentialPolicy(AzureKeyCredential credential, string name)
    {
        Azure.Core.Argument.AssertNotNull(credential, "credential");
        Azure.Core.Argument.AssertNotNullOrEmpty(name, "name");
        _credential = credential;
        _name = name;
    }

    public override void OnSendingRequest(HttpMessage message)
    {
        base.OnSendingRequest(message);
        message.Request.Headers.SetValue(_name, _credential.Key);
    }
}

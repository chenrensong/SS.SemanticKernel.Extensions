// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Azure.Core.Pipeline;

namespace Azure.Core
{
    internal class ProtocolOperation<T> : Operation<T>, IOperation<T> where T : notnull
    {
        private readonly Func<Response, T> _resultSelector;
        private readonly OperationInternal<T> _operation;
        private readonly IOperation _nextLinkOperation;

        internal ProtocolOperation(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Request request, Response response, OperationFinalStateVia finalStateVia, string scopeName, Func<Response, T> resultSelector)
        {
            _resultSelector = resultSelector;
            _nextLinkOperation = NextLinkOperationImplementation.Create(pipeline, request.Method, request.Uri.ToUri(), response, finalStateVia);
            _operation = new OperationInternal<T>(this, clientDiagnostics, response, scopeName);
        }

#pragma warning disable CA1822
        // This scenario is currently unsupported.
        // See: https://github.com/Azure/autorest.csharp/issues/2158.
        /// <inheritdoc />
        public override string Id => throw new NotSupportedException();
#pragma warning restore CA1822

        /// <inheritdoc />
        public override T Value => _operation.Value;

        /// <inheritdoc />
        public override bool HasCompleted => _operation.HasCompleted;

        /// <inheritdoc />
        public override bool HasValue => _operation.HasValue;

        /// <inheritdoc />
        public override Response GetRawResponse() => _operation.RawResponse;

        /// <inheritdoc />
        public override Response UpdateStatus(CancellationToken cancellationToken = default) => _operation.UpdateStatus(cancellationToken);

        /// <inheritdoc />
        public override ValueTask<Response> UpdateStatusAsync(CancellationToken cancellationToken = default) => _operation.UpdateStatusAsync(cancellationToken);

        /// <inheritdoc />
        public override ValueTask<Response<T>> WaitForCompletionAsync(CancellationToken cancellationToken = default) => _operation.WaitForCompletionAsync(cancellationToken);

        /// <inheritdoc />
        public override ValueTask<Response<T>> WaitForCompletionAsync(TimeSpan pollingInterval, CancellationToken cancellationToken = default) => _operation.WaitForCompletionAsync(pollingInterval, cancellationToken);

        async ValueTask<OperationState<T>> IOperation<T>.UpdateStateAsync(bool async, CancellationToken cancellationToken)
        {
            var state = await _nextLinkOperation.UpdateStateAsync(async, cancellationToken).ConfigureAwait(false);
            if (state.HasSucceeded)
            {
                return OperationState<T>.Success(state.RawResponse, _resultSelector(state.RawResponse));
            }

            if (state.HasCompleted)
            {
                return OperationState<T>.Failure(state.RawResponse, state.OperationFailedException);
            }

            return OperationState<T>.Pending(state.RawResponse);
        }
    }


    internal class NextLinkOperationImplementation : IOperation
    {
        private const string ApiVersionParam = "api-version";
        private static readonly string[] FailureStates = { "failed", "canceled" };
        private static readonly string[] SuccessStates = { "succeeded" };

        private readonly HeaderSource _headerSource;
        private readonly bool _originalResponseHasLocation;
        private readonly Uri _startRequestUri;
        private readonly OperationFinalStateVia _finalStateVia;
        private readonly RequestMethod _requestMethod;
        private readonly HttpPipeline _pipeline;
        private readonly string? _apiVersion;

        private string? _lastKnownLocation;
        private string _nextRequestUri;

        public static IOperation Create(
            HttpPipeline pipeline,
            RequestMethod requestMethod,
            Uri startRequestUri,
            Response response,
            OperationFinalStateVia finalStateVia,
            bool skipApiVersionOverride = false,
            string? apiVersionOverrideValue = null)
        {
            string? apiVersionStr = null;
            if (apiVersionOverrideValue is not null)
            {
                apiVersionStr = apiVersionOverrideValue;
            }
            else
            {
                apiVersionStr = !skipApiVersionOverride && TryGetApiVersion(startRequestUri, out ReadOnlySpan<char> apiVersion) ? apiVersion.ToString() : null;
            }
            var headerSource = GetHeaderSource(requestMethod, startRequestUri, response, apiVersionStr, out var nextRequestUri);
            if (headerSource == HeaderSource.None && IsFinalState(response, headerSource, out var failureState, out _))
            {
                return new CompletedOperation(failureState ?? GetOperationStateFromFinalResponse(requestMethod, response));
            }

            var originalResponseHasLocation = response.Headers.TryGetValue("Location", out var lastKnownLocation);
            return new NextLinkOperationImplementation(pipeline, requestMethod, startRequestUri, nextRequestUri, headerSource, originalResponseHasLocation, lastKnownLocation, finalStateVia, apiVersionStr);
        }

        public static IOperation<T> Create<T>(
            IOperationSource<T> operationSource,
            HttpPipeline pipeline,
            RequestMethod requestMethod,
            Uri startRequestUri,
            Response response,
            OperationFinalStateVia finalStateVia,
            bool skipApiVersionOverride = false,
            string? apiVersionOverrideValue = null)
        {
            var operation = Create(pipeline, requestMethod, startRequestUri, response, finalStateVia, skipApiVersionOverride, apiVersionOverrideValue);
            return new OperationToOperationOfT<T>(operationSource, operation);
        }

        private NextLinkOperationImplementation(
            HttpPipeline pipeline,
            RequestMethod requestMethod,
            Uri startRequestUri,
            string nextRequestUri,
            HeaderSource headerSource,
            bool originalResponseHasLocation,
            string? lastKnownLocation,
            OperationFinalStateVia finalStateVia,
            string? apiVersion)
        {
            _requestMethod = requestMethod;
            _headerSource = headerSource;
            _startRequestUri = startRequestUri;
            _nextRequestUri = nextRequestUri;
            _originalResponseHasLocation = originalResponseHasLocation;
            _lastKnownLocation = lastKnownLocation;
            _finalStateVia = finalStateVia;
            _pipeline = pipeline;
            _apiVersion = apiVersion;
        }

        public async ValueTask<OperationState> UpdateStateAsync(bool async, CancellationToken cancellationToken)
        {
            Response response = await GetResponseAsync(async, _nextRequestUri, cancellationToken).ConfigureAwait(false);

            var hasCompleted = IsFinalState(response, _headerSource, out var failureState, out var resourceLocation);
            if (failureState != null)
            {
                return failureState.Value;
            }

            if (hasCompleted)
            {
                string? finalUri = GetFinalUri(resourceLocation);
                var finalResponse = finalUri != null
                    ? await GetResponseAsync(async, finalUri, cancellationToken).ConfigureAwait(false)
                    : response;

                return GetOperationStateFromFinalResponse(_requestMethod, finalResponse);
            }

            UpdateNextRequestUri(response.Headers);
            return OperationState.Pending(response);
        }

        private static OperationState GetOperationStateFromFinalResponse(RequestMethod requestMethod, Response response)
        {
            switch (response.Status)
            {
                case 200:
                case 201 when requestMethod == RequestMethod.Put:
                case 204 when requestMethod != RequestMethod.Put && requestMethod != RequestMethod.Patch:
                    return OperationState.Success(response);
                default:
                    return OperationState.Failure(response);
            }
        }

        private void UpdateNextRequestUri(ResponseHeaders headers)
        {
            var hasLocation = headers.TryGetValue("Location", out string? location);
            if (hasLocation)
            {
                _lastKnownLocation = location;
            }

            switch (_headerSource)
            {
                case HeaderSource.OperationLocation when headers.TryGetValue("Operation-Location", out string? operationLocation):
                    _nextRequestUri = AppendOrReplaceApiVersion(operationLocation, _apiVersion);
                    return;
                case HeaderSource.AzureAsyncOperation when headers.TryGetValue("Azure-AsyncOperation", out string? azureAsyncOperation):
                    _nextRequestUri = AppendOrReplaceApiVersion(azureAsyncOperation, _apiVersion);
                    return;
                case HeaderSource.Location when hasLocation:
                    _nextRequestUri = AppendOrReplaceApiVersion(location!, _apiVersion);
                    return;
            }
        }

        internal static string AppendOrReplaceApiVersion(string uri, string? apiVersion)
        {
            if (!string.IsNullOrEmpty(apiVersion))
            {
                var uriSpan = uri.AsSpan();
                var apiVersionParamSpan = ApiVersionParam.AsSpan();
                var apiVersionIndex = uriSpan.IndexOf(apiVersionParamSpan);
                if (apiVersionIndex == -1)
                {
                    var concatSymbol = uriSpan.IndexOf('?') > -1 ? "&" : "?";
                    return $"{uri}{concatSymbol}api-version={apiVersion}";
                }
                else
                {
                    var lengthToEndOfApiVersionParam = apiVersionIndex + ApiVersionParam.Length;
                    ReadOnlySpan<char> remaining = uriSpan.Slice(lengthToEndOfApiVersionParam);
                    bool apiVersionHasEqualSign = false;
                    if (remaining.IndexOf('=') == 0)
                    {
                        remaining = remaining.Slice(1);
                        lengthToEndOfApiVersionParam += 1;
                        apiVersionHasEqualSign = true;
                    }
                    var indexOfFirstSignAfterApiVersion = remaining.IndexOf('&');
                    ReadOnlySpan<char> uriBeforeApiVersion = uriSpan.Slice(0, lengthToEndOfApiVersionParam);
                    if (indexOfFirstSignAfterApiVersion == -1)
                    {
                        return string.Concat(uriBeforeApiVersion.ToString(), apiVersionHasEqualSign ? string.Empty : "=", apiVersion);
                    }
                    else
                    {
                        ReadOnlySpan<char> uriAfterApiVersion = uriSpan.Slice(indexOfFirstSignAfterApiVersion + lengthToEndOfApiVersionParam);
                        return string.Concat(uriBeforeApiVersion.ToString(), apiVersionHasEqualSign ? string.Empty : "=", apiVersion, uriAfterApiVersion.ToString());
                    }
                }
            }
            return uri;
        }

        internal static bool TryGetApiVersion(Uri startRequestUri, out ReadOnlySpan<char> apiVersion)
        {
            apiVersion = default;
            ReadOnlySpan<char> uriSpan = startRequestUri.Query.AsSpan();
            int startIndex = uriSpan.IndexOf(ApiVersionParam.AsSpan());
            if (startIndex == -1)
            {
                return false;
            }
            startIndex += ApiVersionParam.Length;
            ReadOnlySpan<char> remaining = uriSpan.Slice(startIndex);
            if (remaining.IndexOf('=') == 0)
            {
                remaining = remaining.Slice(1);
                startIndex += 1;
            }
            else
            {
                return false;
            }
            int endIndex = remaining.IndexOf('&');
            int length = endIndex == -1 ? uriSpan.Length - startIndex : endIndex;
            apiVersion = uriSpan.Slice(startIndex, length);
            return true;
        }

        /// <summary>
        /// This function is used to get the final request uri after the lro has completed.
        /// </summary>
        private string? GetFinalUri(string? resourceLocation)
        {
            // Set final uri as null if the response for initial request doesn't contain header "Operation-Location" or "Azure-AsyncOperation".
            if (_headerSource is not (HeaderSource.OperationLocation or HeaderSource.AzureAsyncOperation))
            {
                return null;
            }

            // Set final uri as null if initial request is a delete method.
            if (_requestMethod == RequestMethod.Delete)
            {
                return null;
            }

            // Handle final-state-via options: https://github.com/Azure/autorest/blob/main/docs/extensions/readme.md#x-ms-long-running-operation-options
            switch (_finalStateVia)
            {
                case OperationFinalStateVia.LocationOverride when _originalResponseHasLocation:
                    return _lastKnownLocation;
                case OperationFinalStateVia.OperationLocation or OperationFinalStateVia.AzureAsyncOperation when _requestMethod == RequestMethod.Post:
                    return null;
                case OperationFinalStateVia.OriginalUri:
                    return _startRequestUri.AbsoluteUri;
            }

            // If response body contains resourceLocation, use it: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#target-resource-location
            if (resourceLocation != null)
            {
                return resourceLocation;
            }

            // If initial request is PUT or PATCH, return initial request Uri
            if (_requestMethod == RequestMethod.Put || _requestMethod == RequestMethod.Patch)
            {
                return _startRequestUri.AbsoluteUri;
            }

            // If response for initial request contains header "Location", return last known location
            if (_originalResponseHasLocation)
            {
                return _lastKnownLocation;
            }

            return null;
        }

        private async ValueTask<Response> GetResponseAsync(bool async, string uri, CancellationToken cancellationToken)
        {
            using HttpMessage message = CreateRequest(uri);
            if (async)
            {
                await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _pipeline.Send(message, cancellationToken);
            }
            return message.Response;
        }

        private HttpMessage CreateRequest(string uri)
        {
            HttpMessage message = _pipeline.CreateMessage();
            Request request = message.Request;
            request.Method = RequestMethod.Get;

            if (Uri.TryCreate(uri, UriKind.Absolute, out var nextLink) && nextLink.Scheme != "file")
            {
                request.Uri.Reset(nextLink);
            }
            else
            {
                request.Uri.Reset(new Uri(_startRequestUri, uri));
            }
            return message;
        }

        private static bool IsFinalState(Response response, HeaderSource headerSource, out OperationState? failureState, out string? resourceLocation)
        {
            failureState = null;
            resourceLocation = null;

            if (headerSource == HeaderSource.Location)
            {
                return response.Status != 202;
            }

            if (response.Status is >= 200 and <= 204)
            {
                if (response.ContentStream is { Length: > 0 })
                {
                    try
                    {
                        using JsonDocument document = JsonDocument.Parse(response.ContentStream);
                        var root = document.RootElement;
                        switch (headerSource)
                        {
                            case HeaderSource.None when root.TryGetProperty("properties", out var properties) && properties.TryGetProperty("provisioningState", out JsonElement property):
                            case HeaderSource.OperationLocation when root.TryGetProperty("status", out property):
                            case HeaderSource.AzureAsyncOperation when root.TryGetProperty("status", out property):
                                var state = property.GetRequiredString().ToLowerInvariant();
                                if (FailureStates.Contains(state))
                                {
                                    failureState = OperationState.Failure(response);
                                    return true;
                                }
                                else if (!SuccessStates.Contains(state))
                                {
                                    return false;
                                }
                                else
                                {
                                    if (headerSource is HeaderSource.OperationLocation or HeaderSource.AzureAsyncOperation && root.TryGetProperty("resourceLocation", out var resourceLocationProperty))
                                    {
                                        resourceLocation = resourceLocationProperty.GetString();
                                    }
                                    return true;
                                }
                        }
                    }
                    finally
                    {
                        // It is required to reset the position of the content after reading as this response may be used for deserialization.
                        response.ContentStream.Position = 0;
                    }
                }

                // If headerSource is None and provisioningState was not found, it defaults to Succeeded.
                if (headerSource == HeaderSource.None)
                {
                    return true;
                }
            }

            failureState = OperationState.Failure(response);
            return true;
        }

        private static bool ShouldIgnoreHeader(RequestMethod method, Response response)
            => method.Method == RequestMethod.Patch.Method && response.Status == 200;

        private static HeaderSource GetHeaderSource(RequestMethod requestMethod, Uri requestUri, Response response, string? apiVersion, out string nextRequestUri)
        {
            if (ShouldIgnoreHeader(requestMethod, response))
            {
                nextRequestUri = requestUri.AbsoluteUri;
                return HeaderSource.None;
            }

            var headers = response.Headers;
            if (headers.TryGetValue("Operation-Location", out var operationLocationUri))
            {
                nextRequestUri = AppendOrReplaceApiVersion(operationLocationUri, apiVersion);
                return HeaderSource.OperationLocation;
            }

            if (headers.TryGetValue("Azure-AsyncOperation", out var azureAsyncOperationUri))
            {
                nextRequestUri = AppendOrReplaceApiVersion(azureAsyncOperationUri, apiVersion);
                return HeaderSource.AzureAsyncOperation;
            }

            if (headers.TryGetValue("Location", out var locationUri))
            {
                nextRequestUri = AppendOrReplaceApiVersion(locationUri, apiVersion);
                return HeaderSource.Location;
            }

            nextRequestUri = requestUri.AbsoluteUri;
            return HeaderSource.None;
        }

        private enum HeaderSource
        {
            None,
            OperationLocation,
            AzureAsyncOperation,
            Location
        }

        private class CompletedOperation : IOperation
        {
            private readonly OperationState _operationState;

            public CompletedOperation(OperationState operationState)
            {
                _operationState = operationState;
            }

            public ValueTask<OperationState> UpdateStateAsync(bool async, CancellationToken cancellationToken) => new(_operationState);
        }

        private sealed class OperationToOperationOfT<T> : IOperation<T>
        {
            private readonly IOperationSource<T> _operationSource;
            private readonly IOperation _operation;

            public OperationToOperationOfT(IOperationSource<T> operationSource, IOperation operation)
            {
                _operationSource = operationSource;
                _operation = operation;
            }

            public async ValueTask<OperationState<T>> UpdateStateAsync(bool async, CancellationToken cancellationToken)
            {
                var state = await _operation.UpdateStateAsync(async, cancellationToken).ConfigureAwait(false);
                if (state.HasSucceeded)
                {
                    var result = async
                        ? await _operationSource.CreateResultAsync(state.RawResponse, cancellationToken).ConfigureAwait(false)
                        : _operationSource.CreateResult(state.RawResponse, cancellationToken);

                    return OperationState<T>.Success(state.RawResponse, result);
                }

                if (state.HasCompleted)
                {
                    return OperationState<T>.Failure(state.RawResponse, state.OperationFailedException);
                }

                return OperationState<T>.Pending(state.RawResponse);
            }
        }
    }


    /// <summary>
    /// A helper class used to build long-running operation instances. In order to use this helper:
    /// <list type="number">
    ///   <item>Make sure your LRO implements the <see cref="IOperation"/> interface.</item>
    ///   <item>Add a private <see cref="OperationInternal"/> field to your LRO, and instantiate it during construction.</item>
    ///   <item>Delegate method calls to the <see cref="OperationInternal"/> implementations.</item>
    /// </list>
    /// Supported members:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="OperationInternalBase.HasCompleted"/></description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OperationInternalBase.RawResponse"/>, used for <see cref="Operation.GetRawResponse"/></description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OperationInternalBase.UpdateStatus"/></description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OperationInternalBase.UpdateStatusAsync(CancellationToken)"/></description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OperationInternalBase.WaitForCompletionResponseAsync(CancellationToken)"/></description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OperationInternalBase.WaitForCompletionResponseAsync(TimeSpan, CancellationToken)"/></description>
    ///   </item>
    /// </list>
    /// </summary>
    internal class OperationInternal : OperationInternalBase
    {
        // To minimize code duplication and avoid introduction of another type,
        // OperationInternal delegates implementation to the OperationInternal<VoidValue>.
        // VoidValue is a private empty struct which only purpose is to be used as generic parameter.
        private readonly OperationInternal<VoidValue> _internalOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationInternal"/> class in a final successful state.
        /// </summary>
        /// <param name="rawResponse">The final value of <see cref="OperationInternalBase.RawResponse"/>.</param>
        public static OperationInternal Succeeded(Response rawResponse) => new(OperationState.Success(rawResponse));

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationInternal"/> class in a final failed state.
        /// </summary>
        /// <param name="rawResponse">The final value of <see cref="OperationInternalBase.RawResponse"/>.</param>
        /// <param name="operationFailedException">The exception that will be thrown by <c>UpdateStatusAsync</c>.</param>
        public static OperationInternal Failed(Response rawResponse, RequestFailedException operationFailedException) => new(OperationState.Failure(rawResponse, operationFailedException));

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationInternal"/> class.
        /// </summary>
        /// <param name="operation">The long-running operation making use of this class. Passing "<c>this</c>" is expected.</param>
        /// <param name="clientDiagnostics">Used for diagnostic scope and exception creation. This is expected to be the instance created during the construction of your main client.</param>
        /// <param name="rawResponse">
        ///     The initial value of <see cref="OperationInternalBase.RawResponse"/>. Usually, long-running operation objects can be instantiated in two ways:
        ///     <list type="bullet">
        ///         <item>
        ///             When calling a client's "<c>Start&lt;OperationName&gt;</c>" method, a service call is made to start the operation, and an <see cref="Operation"/> instance is returned.
        ///             In this case, the response received from this service call can be passed here.
        ///         </item>
        ///         <item>
        ///             When a user instantiates an <see cref="Operation"/> directly using a public constructor, there's no previous service call. In this case, passing <c>null</c> is expected.
        ///         </item>
        ///     </list>
        /// </param>
        /// <param name="operationTypeName">
        ///     The type name of the long-running operation making use of this class. Used when creating diagnostic scopes. If left <c>null</c>, the type name will be inferred based on the
        ///     parameter <paramref name="operation"/>.
        /// </param>
        /// <param name="scopeAttributes">The attributes to use during diagnostic scope creation.</param>
        /// <param name="fallbackStrategy"> The delay strategy to use. Default is <see cref="FixedDelayWithNoJitterStrategy"/>.</param>
        public OperationInternal(IOperation operation,
            ClientDiagnostics clientDiagnostics,
            Response rawResponse,
            string? operationTypeName = null,
            IEnumerable<KeyValuePair<string, string>>? scopeAttributes = null,
            DelayStrategy? fallbackStrategy = null)
            : base(clientDiagnostics, operationTypeName ?? operation.GetType().Name, scopeAttributes, fallbackStrategy)
        {
            _internalOperation = new OperationInternal<VoidValue>(new OperationToOperationOfTProxy(operation), clientDiagnostics, rawResponse, operationTypeName ?? operation.GetType().Name, scopeAttributes, fallbackStrategy);
        }

        private OperationInternal(OperationState finalState)
            : base(finalState.RawResponse)
        {
            _internalOperation = finalState.HasSucceeded
                ? OperationInternal<VoidValue>.Succeeded(finalState.RawResponse, default)
                : OperationInternal<VoidValue>.Failed(finalState.RawResponse, finalState.OperationFailedException!);
        }

        public override Response RawResponse => _internalOperation.RawResponse;

        public override bool HasCompleted => _internalOperation.HasCompleted;

        protected override async ValueTask<Response> UpdateStatusAsync(bool async, CancellationToken cancellationToken) =>
            async ? await _internalOperation.UpdateStatusAsync(cancellationToken).ConfigureAwait(false) : _internalOperation.UpdateStatus(cancellationToken);

        // Wrapper type that converts OperationState to OperationState<T> and can be passed to `OperationInternal<T>` constructor.
        private class OperationToOperationOfTProxy : IOperation<VoidValue>
        {
            private readonly IOperation _operation;

            public OperationToOperationOfTProxy(IOperation operation)
            {
                _operation = operation;
            }

            public async ValueTask<OperationState<VoidValue>> UpdateStateAsync(bool async, CancellationToken cancellationToken)
            {
                var state = await _operation.UpdateStateAsync(async, cancellationToken).ConfigureAwait(false);
                if (!state.HasCompleted)
                {
                    return OperationState<VoidValue>.Pending(state.RawResponse);
                }

                if (state.HasSucceeded)
                {
                    return OperationState<VoidValue>.Success(state.RawResponse, new VoidValue());
                }

                return OperationState<VoidValue>.Failure(state.RawResponse, state.OperationFailedException);
            }
        }
    }

    /// <summary>
    /// An interface used by <see cref="OperationInternal"/> for making service calls and updating state. It's expected that
    /// your long-running operation classes implement this interface.
    /// </summary>
    internal interface IOperation
    {
        /// <summary>
        /// Calls the service and updates the state of the long-running operation. Properties directly handled by the
        /// <see cref="OperationInternal"/> class, such as <see cref="OperationInternalBase.RawResponse"/>
        /// don't need to be updated. Operation-specific properties, such as "<c>CreateOn</c>" or "<c>LastModified</c>",
        /// must be manually updated by the operation implementing this method.
        /// <example>Usage example:
        /// <code>
        ///   async ValueTask&lt;OperationState&gt; IOperation.UpdateStateAsync(bool async, CancellationToken cancellationToken)<br/>
        ///   {<br/>
        ///     Response&lt;R&gt; response = async ? &lt;async service call&gt; : &lt;sync service call&gt;;<br/>
        ///     if (&lt;operation succeeded&gt;) return OperationState.Success(response.GetRawResponse(), &lt;parse response&gt;);<br/>
        ///     if (&lt;operation failed&gt;) return OperationState.Failure(response.GetRawResponse());<br/>
        ///     return OperationState.Pending(response.GetRawResponse());<br/>
        ///   }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="async"><c>true</c> if the call should be executed asynchronously. Otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>
        /// A structure indicating the current operation state. The <see cref="OperationState"/> structure must be instantiated by one of
        /// its static methods:
        /// <list type="bullet">
        ///   <item>Use <see cref="OperationState.Success"/> when the operation has completed successfully.</item>
        ///   <item>Use <see cref="OperationState.Failure"/> when the operation has completed with failures.</item>
        ///   <item>Use <see cref="OperationState.Pending"/> when the operation has not completed yet.</item>
        /// </list>
        /// </returns>
        ValueTask<OperationState> UpdateStateAsync(bool async, CancellationToken cancellationToken);
    }

    /// <summary>
    /// A helper structure passed to <see cref="OperationInternal"/> to indicate the current operation state. This structure must be
    /// instantiated by one of its static methods, depending on the operation state:
    /// <list type="bullet">
    ///   <item>Use <see cref="OperationState.Success"/> when the operation has completed successfully.</item>
    ///   <item>Use <see cref="OperationState.Failure"/> when the operation has completed with failures.</item>
    ///   <item>Use <see cref="OperationState.Pending"/> when the operation has not completed yet.</item>
    /// </list>
    /// </summary>
    internal readonly struct OperationState
    {
        private OperationState(Response rawResponse, bool hasCompleted, bool hasSucceeded, RequestFailedException? operationFailedException)
        {
            RawResponse = rawResponse;
            HasCompleted = hasCompleted;
            HasSucceeded = hasSucceeded;
            OperationFailedException = operationFailedException;
        }

        public Response RawResponse { get; }

        public bool HasCompleted { get; }

        public bool HasSucceeded { get; }

        public RequestFailedException? OperationFailedException { get; }

        /// <summary>
        /// Instantiates an <see cref="OperationState"/> indicating the operation has completed successfully.
        /// </summary>
        /// <param name="rawResponse">The HTTP response obtained during the status update.</param>
        /// <returns>A new <see cref="OperationState"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rawResponse"/> is <c>null</c>.</exception>
        public static OperationState Success(Response rawResponse)
        {
            Argument.AssertNotNull(rawResponse, nameof(rawResponse));
            return new OperationState(rawResponse, true, true, default);
        }

        /// <summary>
        /// Instantiates an <see cref="OperationState"/> indicating the operation has completed with failures.
        /// </summary>
        /// <param name="rawResponse">The HTTP response obtained during the status update.</param>
        /// <param name="operationFailedException">
        /// The exception to throw from <c>UpdateStatus</c> because of the operation failure. If left <c>null</c>,
        /// a default exception is created based on the <paramref name="rawResponse"/> parameter.
        /// </param>
        /// <returns>A new <see cref="OperationState"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rawResponse"/> is <c>null</c>.</exception>
        public static OperationState Failure(Response rawResponse, RequestFailedException? operationFailedException = null)
        {
            Argument.AssertNotNull(rawResponse, nameof(rawResponse));
            return new OperationState(rawResponse, true, false, operationFailedException);
        }

        /// <summary>
        /// Instantiates an <see cref="OperationState"/> indicating the operation has not completed yet.
        /// </summary>
        /// <param name="rawResponse">The HTTP response obtained during the status update.</param>
        /// <returns>A new <see cref="OperationState"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rawResponse"/> is <c>null</c>.</exception>
        public static OperationState Pending(Response rawResponse)
        {
            Argument.AssertNotNull(rawResponse, nameof(rawResponse));
            return new OperationState(rawResponse, false, default, default);
        }
    }

    internal interface IOperationSource<T>
    {
        T CreateResult(Response response, CancellationToken cancellationToken);
        ValueTask<T> CreateResultAsync(Response response, CancellationToken cancellationToken);
    }

    internal static class JsonElementExtensions
    {
        public static object? GetObject(in this JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                    {
                        return intValue;
                    }
                    if (element.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    var dictionary = new Dictionary<string, object?>();
                    foreach (JsonProperty jsonProperty in element.EnumerateObject())
                    {
                        dictionary.Add(jsonProperty.Name, jsonProperty.Value.GetObject());
                    }
                    return dictionary;
                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        list.Add(item.GetObject());
                    }
                    return list.ToArray();
                default:
                    throw new NotSupportedException("Not supported value kind " + element.ValueKind);
            }
        }

        public static byte[]? GetBytesFromBase64(in this JsonElement element, string format)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return format switch
            {
                "U" => TypeFormatters.FromBase64UrlString(element.GetRequiredString()),
                "D" => element.GetBytesFromBase64(),
                _ => throw new ArgumentException($"Format is not supported: '{format}'", nameof(format))
            };
        }

        public static DateTimeOffset GetDateTimeOffset(in this JsonElement element, string format) => format switch
        {
            "U" when element.ValueKind == JsonValueKind.Number => DateTimeOffset.FromUnixTimeSeconds(element.GetInt64()),
            // relying on the param check of the inner call to throw ArgumentNullException if GetString() returns null
            _ => TypeFormatters.ParseDateTimeOffset(element.GetString()!, format)
        };

        public static TimeSpan GetTimeSpan(in this JsonElement element, string format) =>
            // relying on the param check of the inner call to throw ArgumentNullException if GetString() returns null
            TypeFormatters.ParseTimeSpan(element.GetString()!, format);

        public static char GetChar(this in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var text = element.GetString();
                if (text == null || text.Length != 1)
                {
                    throw new NotSupportedException($"Cannot convert \"{text}\" to a Char");
                }
                return text[0];
            }
            else
            {
                throw new NotSupportedException($"Cannot convert {element.ValueKind} to a Char");
            }
        }

        public static void ThrowNonNullablePropertyIsNull(this JsonProperty property)
        {
            throw new JsonException($"A property '{property.Name}' defined as non-nullable but received as null from the service. " +
                                    $"This exception only happens in DEBUG builds of the library and would be ignored in the release build");
        }

        public static string GetRequiredString(in this JsonElement element)
        {
            var value = element.GetString();
            if (value == null)
                throw new InvalidOperationException($"The requested operation requires an element of type 'String', but the target element has type '{element.ValueKind}'.");

            return value;
        }
    }

    internal class TypeFormatters
    {
        private const string RoundtripZFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
        public static string DefaultNumberFormat { get; } = "G";

        public static string ToString(bool value) => value ? "true" : "false";

        public static string ToString(DateTime value, string format) => value.Kind switch
        {
            DateTimeKind.Utc => ToString((DateTimeOffset)value, format),
            _ => throw new NotSupportedException($"DateTime {value} has a Kind of {value.Kind}. Azure SDK requires it to be UTC. You can call DateTime.SpecifyKind to change Kind property value to DateTimeKind.Utc.")
        };

        public static string ToString(DateTimeOffset value, string format) => format switch
        {
            "D" => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            "U" => value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            "O" => value.ToUniversalTime().ToString(RoundtripZFormat, CultureInfo.InvariantCulture),
            "o" => value.ToUniversalTime().ToString(RoundtripZFormat, CultureInfo.InvariantCulture),
            "R" => value.ToString("r", CultureInfo.InvariantCulture),
            _ => value.ToString(format, CultureInfo.InvariantCulture)
        };

        public static string ToString(TimeSpan value, string format) => format switch
        {
            "P" => XmlConvert.ToString(value),
            _ => value.ToString(format, CultureInfo.InvariantCulture)
        };

        public static string ToString(byte[] value, string format) => format switch
        {
            "U" => ToBase64UrlString(value),
            "D" => Convert.ToBase64String(value),
            _ => throw new ArgumentException($"Format is not supported: '{format}'", nameof(format))
        };

        public static string ToBase64UrlString(byte[] value)
        {
            var numWholeOrPartialInputBlocks = checked(value.Length + 2) / 3;
            var size = checked(numWholeOrPartialInputBlocks * 4);
            var output = new char[size];

            var numBase64Chars = Convert.ToBase64CharArray(value, 0, value.Length, output, 0);

            // Fix up '+' -> '-' and '/' -> '_'. Drop padding characters.
            int i = 0;
            for (; i < numBase64Chars; i++)
            {
                var ch = output[i];
                if (ch == '+')
                {
                    output[i] = '-';
                }
                else if (ch == '/')
                {
                    output[i] = '_';
                }
                else if (ch == '=')
                {
                    // We've reached a padding character; truncate the remainder.
                    break;
                }
            }

            return new string(output, 0, i);
        }

        public static byte[] FromBase64UrlString(string value)
        {
            var paddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(value.Length);

            var output = new char[value.Length + paddingCharsToAdd];

            int i;
            for (i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch == '-')
                {
                    output[i] = '+';
                }
                else if (ch == '_')
                {
                    output[i] = '/';
                }
                else
                {
                    output[i] = ch;
                }
            }

            for (; i < output.Length; i++)
            {
                output[i] = '=';
            }

            return Convert.FromBase64CharArray(output, 0, output.Length);
        }

        private static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
        {
            switch (inputLength % 4)
            {
                case 0:
                    return 0;
                case 2:
                    return 2;
                case 3:
                    return 1;
                default:
                    throw new InvalidOperationException("Malformed input");
            }
        }

        public static DateTimeOffset ParseDateTimeOffset(string value, string format)
        {
            return format switch
            {
                "U" => DateTimeOffset.FromUnixTimeSeconds(long.Parse(value, CultureInfo.InvariantCulture)),
                _ => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            };
        }

        public static TimeSpan ParseTimeSpan(string value, string format) => format switch
        {
            "P" => XmlConvert.ToTimeSpan(value),
            _ => TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture)
        };

        public static string ConvertToString(object? value, string? format = null)
            => value switch
            {
                null => "null",
                string s => s,
                bool b => ToString(b),
                int or float or double or long or decimal => ((IFormattable)value).ToString(DefaultNumberFormat, CultureInfo.InvariantCulture),
                byte[] b when format != null => ToString(b, format),
                IEnumerable<string> s => string.Join(",", s),
                DateTimeOffset dateTime when format != null => ToString(dateTime, format),
                TimeSpan timeSpan when format != null => ToString(timeSpan, format),
                TimeSpan timeSpan => XmlConvert.ToString(timeSpan),
                Guid guid => guid.ToString(),
                BinaryData binaryData => TypeFormatters.ConvertToString(binaryData.ToArray(), format),
                _ => value.ToString()!
            };
    }
}

﻿using System;
using System.ComponentModel;

namespace Azure.AI.OpenAI
{
    internal readonly partial struct AzureOpenAIOperationState : IEquatable<AzureOpenAIOperationState>
    {
        private readonly string _value;

        /// <summary> Initializes a new instance of <see cref="AzureOpenAIOperationState"/>. </summary>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public AzureOpenAIOperationState(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private const string NotRunningValue = "notRunning";
        private const string RunningValue = "running";
        private const string SucceededValue = "succeeded";
        private const string CanceledValue = "canceled";
        private const string FailedValue = "failed";

        /// <summary> The operation was created and is queued to be processed in the future. </summary>
        public static AzureOpenAIOperationState NotRunning { get; } = new AzureOpenAIOperationState(NotRunningValue);
        /// <summary> The operation has started to be processed. </summary>
        public static AzureOpenAIOperationState Running { get; } = new AzureOpenAIOperationState(RunningValue);
        /// <summary> The operation has successfully be processed and is ready for consumption. </summary>
        public static AzureOpenAIOperationState Succeeded { get; } = new AzureOpenAIOperationState(SucceededValue);
        /// <summary> The operation has been canceled and is incomplete. </summary>
        public static AzureOpenAIOperationState Canceled { get; } = new AzureOpenAIOperationState(CanceledValue);
        /// <summary> The operation has completed processing with a failure and cannot be further consumed. </summary>
        public static AzureOpenAIOperationState Failed { get; } = new AzureOpenAIOperationState(FailedValue);
        /// <summary> Determines if two <see cref="AzureOpenAIOperationState"/> values are the same. </summary>
        public static bool operator ==(AzureOpenAIOperationState left, AzureOpenAIOperationState right) => left.Equals(right);
        /// <summary> Determines if two <see cref="AzureOpenAIOperationState"/> values are not the same. </summary>
        public static bool operator !=(AzureOpenAIOperationState left, AzureOpenAIOperationState right) => !left.Equals(right);
        /// <summary> Converts a string to a <see cref="AzureOpenAIOperationState"/>. </summary>
        public static implicit operator AzureOpenAIOperationState(string value) => new AzureOpenAIOperationState(value);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is AzureOpenAIOperationState other && Equals(other);
        /// <inheritdoc />
        public bool Equals(AzureOpenAIOperationState other) => string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        /// <inheritdoc />
        public override string ToString() => _value;
    }

}

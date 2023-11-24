using System;
using System.ComponentModel;

namespace Azure.AI.OpenAI
{
    internal struct CoreFunctionCallPreset : IEquatable<CoreFunctionCallPreset>
    {
        private readonly string _value;

        private const string AutoValue = "auto";

        private const string NoneValue = "none";

        public static CoreFunctionCallPreset Auto { get; } = new CoreFunctionCallPreset("auto");


        public static CoreFunctionCallPreset None { get; } = new CoreFunctionCallPreset("none");


        public CoreFunctionCallPreset(string value)
        {
            _value = value ?? throw new ArgumentNullException("value");
        }

        public static bool operator ==(CoreFunctionCallPreset left, CoreFunctionCallPreset right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CoreFunctionCallPreset left, CoreFunctionCallPreset right)
        {
            return !left.Equals(right);
        }

        public static implicit operator CoreFunctionCallPreset(string value)
        {
            return new CoreFunctionCallPreset(value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is CoreFunctionCallPreset other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(CoreFunctionCallPreset other)
        {
            return string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _value;
        }
    }


}

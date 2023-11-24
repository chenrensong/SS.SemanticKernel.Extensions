using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Azure.Core
{
    internal interface IUtf8JsonSerializable
    {
        void Write(Utf8JsonWriter writer);
    }

}

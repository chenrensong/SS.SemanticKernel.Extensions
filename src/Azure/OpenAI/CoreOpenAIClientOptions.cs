// Azure.AI.OpenAI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=92742159e12e44c8
// Azure.AI.OpenAI.OpenAIClientOptions
using System;
using Azure.AI.OpenAI;
using Azure.Core;

namespace Azure.AI.OpenAI
{
    public class CoreOpenAIClientOptions : OpenAIClientOptions
    {

        private const ServiceVersion LatestVersion = ServiceVersion.V2023_09_01_Preview;

        internal string Version { get; }

        public CoreOpenAIClientOptions(ServiceVersion version = ServiceVersion.V2023_09_01_Preview)
        {
            string text;
            switch (version)
            {
                case ServiceVersion.V2022_12_01:
                    text = "2022-12-01";
                    break;
                case ServiceVersion.V2023_05_15:
                    text = "2023-05-15";
                    break;
                case ServiceVersion.V2023_06_01_Preview:
                    text = "2023-06-01-preview";
                    break;
                case ServiceVersion.V2023_07_01_Preview:
                    text = "2023-07-01-preview";
                    break;
                case ServiceVersion.V2023_08_01_Preview:
                    text = "2023-08-01-preview";
                    break;
                case ServiceVersion.V2023_09_01_Preview:
                    text = "2023-09-01-preview";
                    break;
                default:
                    throw new NotSupportedException();
            }
            Version = text;
        }
    }

}
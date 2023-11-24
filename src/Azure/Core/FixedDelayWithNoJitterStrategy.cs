// Azure.AI.OpenAI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=92742159e12e44c8
// Azure.Core.FixedDelayWithNoJitterStrategy
using System;
using System.Runtime.CompilerServices;
using Azure;
using Azure.Core;
namespace Azure.Core
{

    internal class FixedDelayWithNoJitterStrategy : DelayStrategy
    {
        private static readonly TimeSpan DefaultDelay = TimeSpan.FromSeconds(1.0);

        private readonly TimeSpan _delay;

        public FixedDelayWithNoJitterStrategy(TimeSpan? suggestedDelay = null)
            : base(suggestedDelay.HasValue ? DelayStrategy.Max(suggestedDelay.Value, DefaultDelay) : DefaultDelay, 0.0)
        {
            _delay = (suggestedDelay.HasValue ? DelayStrategy.Max(suggestedDelay.Value, DefaultDelay) : DefaultDelay);
        }

        protected override TimeSpan GetNextDelayCore(Response response, int retryNumber)
        {
            return _delay;
        }
    }

}

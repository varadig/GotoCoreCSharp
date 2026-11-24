using System;
using GotoCore.Model;

namespace GotoCore.Samples.Counter
{
    // ---- Messages: plain readonly structs. Views publish intents, controllers publish events. ----

    public readonly struct IncrementRequested
    {
        public IncrementRequested(int amount) => Amount = amount;
        public int Amount { get; }
    }

    public readonly struct ResetRequested
    {
    }

    public readonly struct MilestoneReached
    {
        public MilestoneReached(int value) => Value = value;
        public int Value { get; }
    }

    // ---- Model: state + rules, no Unity and no messaging. Views only see the read-only property. ----

    public sealed class CounterModel
    {
        private readonly ObservableProperty<int> _count = new ObservableProperty<int>(0);

        public IReadOnlyObservableProperty<int> Count => _count;

        public void Set(int value) => _count.Value = Math.Max(0, value);
    }
}

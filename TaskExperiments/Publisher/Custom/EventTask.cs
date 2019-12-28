using System;
using System.Runtime.CompilerServices;

namespace TaskExperiments.Publisher.Custom
{
    public readonly struct EventTask : INotifyCompletion
    {
        private readonly AwaitableEvent _awaitableEvent;
        private readonly long _sequence;

        public EventTask(AwaitableEvent awaitableEvent, long sequence)
        {
            _awaitableEvent = awaitableEvent;
            _sequence = sequence;
        }

        public EventTask GetAwaiter() => this;

        public bool IsCompleted => _awaitableEvent.IsCompleted(_sequence);

        public void GetResult()
        {
        }
        
        public void OnCompleted(Action continuation)
        {
            _awaitableEvent.RegisterContinuation(_sequence, continuation);
        }
    }
}
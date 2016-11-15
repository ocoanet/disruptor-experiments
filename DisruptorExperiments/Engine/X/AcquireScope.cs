using System;
using Disruptor;

namespace DisruptorExperiments.Engine.X
{
    public class AcquireScope<TEvent> : IDisposable where TEvent : class
    {
        private ISequenced _ringBuffer;
        private long _sequence;

        public AcquireScope(TEvent evt)
        {
            Event = evt;
        }

        public TEvent Event { get; }

        public void OnAcquired(ISequenced ringBuffer, long sequence)
        {
            _ringBuffer = ringBuffer;
            _sequence = sequence;
        }

        public void Dispose() => _ringBuffer.Publish(_sequence);
    }
}
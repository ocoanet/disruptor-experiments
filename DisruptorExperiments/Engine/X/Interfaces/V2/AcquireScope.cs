using System;
using Disruptor;

namespace DisruptorExperiments.Engine.X.Interfaces.V2
{
    public struct AcquireScope<TEvent> : IDisposable where TEvent : class
    {
        private RingBuffer<TEvent> _ringBuffer;
        private long _sequence;

        public AcquireScope(RingBuffer<TEvent> ringBuffer, long sequence, TEvent data)
        {
            _ringBuffer = ringBuffer;
            _sequence = sequence;

            Event = data;
        }

        public TEvent Event;

        public void Dispose() => _ringBuffer.Publish(_sequence);
    }
}
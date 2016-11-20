using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Disruptor;
using DisruptorExperiments.Engine.X.Interfaces.V2;

namespace DisruptorExperiments.Tests.Engine
{
    public class TestEngine<TEvent> where TEvent : class
    {
        private readonly ManualResetEventSlim _acquiredSignal = new ManualResetEventSlim();
        private readonly List<TEvent> _acquiredEvents = new List<TEvent>();
        private readonly Func<TEvent> _eventFactory;
        private readonly AcquireScope<TEvent> _dummyScope;
        private int _acquiredEventCount;

        public TestEngine(Func<TEvent> eventFactory)
        {
            _eventFactory = eventFactory;
            _dummyScope = CreateAcquireScope(x => Interlocked.Increment(ref _acquiredEventCount));
        }

        public bool AcquiredEntriesRecordingEnabled { get; set; } = true;
        public int AcquiredEventCount => _acquiredEventCount;

        public List<TEvent> GetAcquiredEvents()
        {
            lock (_acquiredEvents)
            {
                return _acquiredEvents.ToList();
            }
        }

        public TEvent GetLastEvent()
        {
            lock (_acquiredEvents)
            {
                return _acquiredEvents.LastOrDefault();
            }
        }

        public List<TEvent> ResetAcquiredEvents()
        {
            lock (_acquiredEvents)
            {
                var acquiredEvents = _acquiredEvents.ToList();
                _acquiredEvents.Clear();
                return acquiredEvents;
            }
        }

        public AcquireScope<TEvent> AcquireEvent()
        {
            if (AcquiredEntriesRecordingEnabled)
                return CreateAcquireScope(OnEventPublished);

            return _dummyScope;
        }

        private void OnEventPublished(TEvent data)
        {
            lock (_acquiredEvents)
            {
                _acquiredEvents.Add(data);
            }
            Interlocked.Increment(ref _acquiredEventCount);

            _acquiredSignal.Set();
        }

        private AcquireScope<TEvent> CreateAcquireScope(Action<TEvent> onEventPublished)
        {
            var sequenced = new TestSequenced();
            var rinBuffer = new RingBuffer<TEvent>(_eventFactory, sequenced);
            sequenced.OnPublished = () => onEventPublished.Invoke(rinBuffer[0]);

            return new AcquireScope<TEvent>(rinBuffer, 0, rinBuffer[0]);
        }

        private class TestSequenced : ISequencer
        {
            public int BufferSize => 1;
            public long Cursor => 0;
            public Action OnPublished { get; set; }

            public void Publish(long sequence)
            {
                var onPublished = OnPublished;
                onPublished?.Invoke();
            }

            public bool HasAvailableCapacity(int requiredCapacity)
            {
                throw new NotSupportedException();
            }

            public long GetRemainingCapacity()
            {
                throw new NotSupportedException();
            }

            public long Next()
            {
                throw new NotSupportedException();
            }

            public long Next(int n)
            {
                throw new NotSupportedException();
            }

            public long TryNext()
            {
                throw new NotSupportedException();
            }

            public long TryNext(int n)
            {
                throw new NotSupportedException();
            }

            public void Publish(long lo, long hi)
            {
                throw new NotSupportedException();
            }

            public void Claim(long sequence)
            {
                throw new NotSupportedException();
            }

            public bool IsAvailable(long sequence)
            {
                throw new NotSupportedException();
            }

            public void AddGatingSequences(params ISequence[] gatingSequences)
            {
                throw new NotSupportedException();
            }

            public bool RemoveGatingSequence(ISequence sequence)
            {
                throw new NotSupportedException();
            }

            public ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack)
            {
                throw new NotSupportedException();
            }

            public long GetMinimumSequence()
            {
                throw new NotSupportedException();
            }

            public long GetHighestPublishedSequence(long nextSequence, long availableSequence)
            {
                throw new NotSupportedException();
            }

            public EventPoller<T> NewPoller<T>(IDataProvider<T> provider, params ISequence[] gatingSequences)
            {
                throw new NotSupportedException();
            }
        }
    }
}
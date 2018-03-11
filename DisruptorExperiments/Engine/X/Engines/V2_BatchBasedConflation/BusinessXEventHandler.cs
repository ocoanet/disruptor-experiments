using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V2_BatchBasedConflation
{
    public class BusinessXEventHandler : IEventHandler<XEvent>
    {
        private readonly Stack<MovingAverage> _movingAveragePool = new Stack<MovingAverage>(Enumerable.Range(0, 100).Select(x => new MovingAverage()));
        private readonly Dictionary<int, MovingAverage> _movingAverages = new Dictionary<int, MovingAverage>();
        private readonly Batch _batch = new Batch();

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            data.HandlerMetrics[0].BeginTimestamp = Stopwatch.GetTimestamp();

            AddToBatch(data);

            if (endOfBatch)
                ProcessBatch();

            data.HandlerMetrics[0].EndTimestamp = Stopwatch.GetTimestamp();
        }

        private void AddToBatch(XEvent data)
        {
            _batch.Add(data);
        }

        private void ProcessBatch()
        {
            for (int entryIndex = 0; entryIndex < _batch.Events.Count; entryIndex++)
            {
                var entry = _batch.Events[entryIndex];

                if (entry.EventType == XEventType.MarketData)
                {
                    ProcessMarketDataUpdate(ref entry.EventData.MarketData);
                }
            }
            _batch.Clear();
        }

        private void ProcessMarketDataUpdate(ref XEvent.MarketDataInfo marketData)
        {
            Thread.SpinWait(1 << 5);

            if (marketData.LastOrZero == 0)
                return;

            var movingAverage = GetMovingAverage(marketData.SecurityId);
            movingAverage.Add(marketData.LastOrZero);
        }

        private MovingAverage GetMovingAverage(int securityId)
        {
            MovingAverage movingAverage;
            if (_movingAverages.TryGetValue(securityId, out movingAverage))
                return movingAverage;

            movingAverage = _movingAveragePool.Count != 0 ? _movingAveragePool.Pop() : new MovingAverage();
            _movingAverages.Add(securityId, movingAverage);

            return movingAverage;
        }

        private class Batch
        {
            private readonly Dictionary<int, XEvent> _marketDataEvents = new Dictionary<int, XEvent>();
            public readonly List<XEvent> Events = new List<XEvent>();

            public void Add(XEvent data)
            {
                if (data.EventType == XEventType.MarketData)
                {
                    XEvent previousEvent;
                    if (_marketDataEvents.TryGetValue(data.EventData.MarketData.SecurityId, out previousEvent))
                    {
                        data.EventData.MarketData.ApplyTo(ref previousEvent.EventData.MarketData);
                        data.EventType = XEventType.None;
                        return;
                    }
                    _marketDataEvents.Add(data.EventData.MarketData.SecurityId, data);
                }
                Events.Add(data);
            }

            public void Clear()
            {
                _marketDataEvents.Clear();
                Events.Clear();
            }
        }

        private class MovingAverage
        {
            private const int _capacity = 100;

            public Queue<long> Values = new Queue<long>(_capacity);
            public long Sum;
            public long Value;

            public void Add(long value)
            {
                if (Values.Count == _capacity)
                {
                    var dequeued = Values.Dequeue();
                    Sum -= dequeued;
                }
                Values.Enqueue(value);
                Sum += value;
                Value = Sum / Values.Count;
            }
        }
    }
}
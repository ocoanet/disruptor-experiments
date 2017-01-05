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

            _batch.Add(data);

            if (endOfBatch)
                ProcessBatch();

            data.HandlerMetrics[0].EndTimestamp = Stopwatch.GetTimestamp();
        }

        private void ProcessBatch()
        {
            for (int entryIndex = 0; entryIndex < _batch.Entries.Count; entryIndex++)
            {
                var entry = _batch.Entries[entryIndex];

                if (entry.EventType == XEventType.MarketData)
                {
                    ProcessMarketDataUpdate(ref entry.EventData.MarketData);
                }
            }
            _batch.Clear();
        }

        private void ProcessMarketDataUpdate(ref XEvent.MarketDataInfo marketData)
        {
            var marketDataUpdate = marketData.Update;

            Thread.SpinWait(1 << 5);

            if (marketDataUpdate.Last == null)
                return;

            var movingAverage = GetMovingAverage(marketData.SecurityId);
            movingAverage.Add(marketDataUpdate.Last.Value);
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
            private readonly Dictionary<int, XEvent> _marketDataEntries = new Dictionary<int, XEvent>();
            public readonly List<XEvent> Entries = new List<XEvent>();

            public void Add(XEvent data)
            {
                if (data.EventType == XEventType.MarketData)
                {
                    XEvent previousMarketDataEvent;
                    if (_marketDataEntries.TryGetValue(data.EventData.MarketData.SecurityId, out previousMarketDataEvent))
                    {
                        data.MarketDataUpdate.ApplyTo(previousMarketDataEvent.MarketDataUpdate);
                        data.EventType = XEventType.None;
                        return;
                    }
                    _marketDataEntries.Add(data.EventData.MarketData.SecurityId, data);
                }
                Entries.Add(data);
            }

            public void Clear()
            {
                _marketDataEntries.Clear();
                Entries.Clear();
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
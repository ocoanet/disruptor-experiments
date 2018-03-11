using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V1_SyncBasedConflation
{
    public class BusinessXEventHandler : IEventHandler<XEvent>
    {
        private readonly Stack<MovingAverage> _movingAveragePool = new Stack<MovingAverage>(Enumerable.Range(0, 100).Select(x => new MovingAverage()));
        private readonly Dictionary<int, MovingAverage> _movingAverages = new Dictionary<int, MovingAverage>();

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            data.HandlerMetrics[0].BeginTimestamp = Stopwatch.GetTimestamp();

            if (data.EventType == XEventType.MarketData)
            {
                ProcessMarketDataUpdate(ref data.EventData.MarketData);
            }

            data.HandlerMetrics[0].EndTimestamp = Stopwatch.GetTimestamp();
        }

        private void ProcessMarketDataUpdate(ref XEvent.MarketDataInfo marketData)
        {
            marketData.Conflater.Detach();

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
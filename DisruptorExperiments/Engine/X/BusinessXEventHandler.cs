using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Disruptor;

namespace DisruptorExperiments.Engine.X
{
    public class BusinessXEventHandler : IEventHandler<XEvent>
    {
        private readonly Stack<MovingAverage> _movingAveragePool = new Stack<MovingAverage>(Enumerable.Range(0, 100).Select(x => new MovingAverage()));
        private readonly Dictionary<int, MovingAverage> _movingAverages = new Dictionary<int, MovingAverage>();

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            data.HandlerBeginTimestamps[0] = Stopwatch.GetTimestamp();

            if (data.EventType == XEventType.MarketDataUpdate)
            {
                ProcessMarketDataUpdate(data);
            }

            data.HandlerEndTimestamps[0] = Stopwatch.GetTimestamp();
        }

        private void ProcessMarketDataUpdate(XEvent data)
        {
            data.MarketDataConflater.Detach();

            Thread.SpinWait(1 << 6);

            if (data.MarketDataUpdate.Last == null)
                return;

            var movingAverage = GetMovingAverage(data.MarketDataUpdate.SecurityId);
            movingAverage.Add(data.MarketDataUpdate.Last.Value);
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
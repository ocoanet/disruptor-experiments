using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Disruptor;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments.Engine.X.Engines.V2_BatchBasedConflation
{
    public class BatchingBusinessXEventHandler : IEventHandler<XEvent>
    {
        private readonly Stack<Security> _securityPool = new Stack<Security>(Enumerable.Range(0, 100).Select(x => new Security()));
        private readonly Dictionary<int, Security> _securities = new Dictionary<int, Security>(100);
        private readonly Dictionary<int, Security> _updatedSecurities = new Dictionary<int, Security>(100);
        private long _marketDataEntryCount = 0;

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            data.HandlerMetrics[0].BeginTimestamp = Stopwatch.GetTimestamp();

            if (data.EventType == XEventType.MarketData)
            {
                AddToUpdatedSecurities(ref data.EventData.MarketData, data.MarketDataUpdate);
            }

            if (_updatedSecurities.Count != 0 && (data.EventType != XEventType.MarketData || endOfBatch))
            {
                ProcessUpdatedSecurities();
                data.ProcessedMarketDataCount = _marketDataEntryCount;
                _marketDataEntryCount = 0;
            }

            data.HandlerMetrics[0].EndTimestamp = Stopwatch.GetTimestamp();
        }

        private void AddToUpdatedSecurities(ref XEvent.MarketDataInfo marketData, MarketDataUpdate update)
        {
            var security = GetSecurity(marketData.SecurityId);
            update.ApplyTo(security.MarketData);
            _updatedSecurities[marketData.SecurityId] = security;
            _marketDataEntryCount++;
        }

        private Security GetSecurity(int securityId)
        {
            Security security;
            if (_securities.TryGetValue(securityId, out security))
                return security;

            security = _securityPool.Count != 0 ? _securityPool.Pop() : new Security();
            security.SecurityId = securityId;

            _securities.Add(securityId, security);

            return security;
        }

        private void ProcessUpdatedSecurities()
        {
            foreach (var security in _updatedSecurities.Values)
            {
                Thread.SpinWait(1 << 5);

                if (security.MarketData.Last == null)
                    return;

                security.MovingAverage.Add(security.MarketData.Last.Value);
            }

            _updatedSecurities.Clear();
        }

        private class Security
        {
            public int SecurityId { get; set; }
            public MarketDataUpdate MarketData { get; } = new MarketDataUpdate();
            public MovingAverage MovingAverage { get; } = new MovingAverage();
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
using System.Diagnostics;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments.Engine.X
{
    public class XEvent
    {
        public readonly MarketDataUpdate MarketDataUpdate = new MarketDataUpdate();
        public readonly long[] HandlerBeginTimestamps = new long[5];
        public readonly long[] HandlerEndTimestamps = new long[5];

        public XEventType EventType;
        public int MarketDataSecurityId;
        public IMarketDataConflater MarketDataConflater;
        public long AcquireTimestamp;

        public void OnAcquired()
        {
            AcquireTimestamp = Stopwatch.GetTimestamp();
        }

        public void Reset()
        {
            EventType = XEventType.None;
            MarketDataSecurityId = 0;
            MarketDataConflater = null;
            MarketDataUpdate.Reset();
        }

        public void SetMarketDataUpdate(int securityId, IMarketDataConflater marketDataConflater)
        {
            EventType = XEventType.MarketDataUpdate;
            MarketDataSecurityId = securityId;
            MarketDataConflater = marketDataConflater;
        }
    }
}
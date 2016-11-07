using System.Diagnostics;
using DisruptorExperiments.MarketData.V1;

namespace DisruptorExperiments.Engine.X
{
    public class XEvent
    {
        public readonly MarketDataUpdate MarketDataUpdate = new MarketDataUpdate();
        public readonly long[] HandlerBeginTimestamps = new long[5];
        public readonly long[] HandlerEndTimestamps = new long[5];

        public XEventType EventType;
        public MarketDataConflater MarketDataConflater;
        public long AcquireTimestamp;

        public void OnAcquired()
        {
            AcquireTimestamp = Stopwatch.GetTimestamp();
        }

        public void Reset()
        {
            EventType = XEventType.None;
            MarketDataConflater = null;
            MarketDataUpdate.Reset();
        }
    }
}
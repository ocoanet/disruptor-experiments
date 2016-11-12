using System.Runtime.InteropServices;

namespace DisruptorExperiments.Engine.X.Events.V3
{
    public class XEvent
    {
        public XEventType EventType { get; private set; }
        public EventInfo EventData;

        public void SetMarketDataUpdate(int securityId, long bidPrice, long askPrice)
        {
            EventType = XEventType.MarketDataUpdate;
            EventData.MarketDataUpdate.SecurityId = securityId;
            EventData.MarketDataUpdate.BidPrice = bidPrice;
            EventData.MarketDataUpdate.AskPrice = askPrice;
        }

        public void SetExecution(int securityId, long price, long quantity)
        {
            EventType = XEventType.Execution;
            EventData.Execution.SecurityId = securityId;
            EventData.Execution.Price = price;
            EventData.Execution.Quantity = quantity;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct EventInfo
        {
            [FieldOffset(0)]
            public ExecutionInfo Execution;

            [FieldOffset(0)]
            public MarketDataUpdateInfo MarketDataUpdate;
        }

        [StructLayout(LayoutKind.Explicit)]
        public class ExecutionInfo
        {
            [FieldOffset(0)]
            public int SecurityId;
            [FieldOffset(4)]
            public long Price;
            [FieldOffset(12)]
            public long Quantity;
        }

        [StructLayout(LayoutKind.Explicit)]
        public class MarketDataUpdateInfo
        {
            [FieldOffset(0)]
            public int SecurityId;
            [FieldOffset(4)]
            public long BidPrice;
            [FieldOffset(12)]
            public long AskPrice;
        }
    }
}
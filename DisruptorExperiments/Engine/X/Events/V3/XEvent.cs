using System.Runtime.InteropServices;

namespace DisruptorExperiments.Engine.X.Events.V3
{
    public class XEvent
    {
        public XEventType EventType { get; private set; }
        public EventInfo EventData;

        public void SetMarketData(int securityId, long bidPrice, long askPrice)
        {
            EventType = XEventType.MarketData;
            EventData.MarketData.SecurityId = securityId;
            EventData.MarketData.BidPrice = bidPrice;
            EventData.MarketData.AskPrice = askPrice;
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
            public MarketDataInfo MarketData;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ExecutionInfo
        {
            [FieldOffset(0)]
            public int SecurityId;
            [FieldOffset(4)]
            public long Price;
            [FieldOffset(12)]
            public long Quantity;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MarketDataInfo
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
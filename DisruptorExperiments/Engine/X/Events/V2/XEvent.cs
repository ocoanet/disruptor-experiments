namespace DisruptorExperiments.Engine.X.Events.V2
{
    public class XEvent
    {
        public XEventType EventType { get; private set; }
        public object EventData { get; private set; }

        public void SetMarketData(MarketDataInfo marketData)
        {
            EventType = XEventType.MarketData;
            EventData = marketData;
        }

        public void SetExecution(ExecutionInfo execution)
        {
            EventType = XEventType.Execution;
            EventData = execution;
        }

        public class ExecutionInfo
        {
            public int SecurityId;
            public long Price;
            public long Quantity;
        }

        public class MarketDataInfo
        {
            public int SecurityId;
            public long BidPrice;
            public long AskPrice;
        }
    }
}
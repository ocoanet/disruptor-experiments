namespace DisruptorExperiments.Engine.X.Events.V1_PropertyMess
{
    public class XEvent
    {
        public XEventType EventType;
        public MarketDataInfo MarketData;
        public ExecutionInfo Execution;
        public TradingSignal1Info TradingSignal1;

        public struct MarketDataInfo
        {
            public int SecurityId;
            public long BidPrice;
            public long AskPrice;
        }

        public struct ExecutionInfo
        {
            public int SecurityId;
            public long Price;
            public long Quantity;
        }

        public struct TradingSignal1Info
        {
        }

        public void SetMarketData(int securityId, long bidPrice, long askPrice)
        {
            EventType = XEventType.MarketData;
            MarketData.SecurityId = securityId;
            MarketData.BidPrice = bidPrice;
            MarketData.AskPrice = askPrice;
        }

        public void SetExecution(int securityId, long price, long quantity)
        {
            EventType = XEventType.Execution;
            Execution.SecurityId = securityId;
            Execution.Price = price;
            Execution.Quantity = quantity;
        }
    }
}
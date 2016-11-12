namespace DisruptorExperiments.Engine.X.Events.V1
{
    public class XEvent
    {
        public XEventType EventType;
        public int MarketDataSecurityId;
        public long MarketDataBidPrice;
        public long MarketDataAskPrice;
        public int ExecutionSecurityId;
        public long ExecutionPrice;
        public long ExecutionQuantity;

        public void SetMarketDataUpdate(int securityId, long bidPrice, long askPrice)
        {
            EventType = XEventType.MarketDataUpdate;
            MarketDataSecurityId = securityId;
            MarketDataBidPrice = bidPrice;
            MarketDataAskPrice = askPrice;
        }

        public void SetExecution(int securityId, long price, long quantity)
        {
            EventType = XEventType.Execution;
            ExecutionSecurityId = securityId;
            ExecutionPrice = price;
            ExecutionQuantity = quantity;
        }
    }
}
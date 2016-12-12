namespace DisruptorExperiments.Engine.X.Interfaces.V1_MethodPerEventType
{
    public interface IXEngine
    {
        void EnqueueMarketData(int securityId, long bidPrice, long askPrice);
        void EnqueueExecution(int securityId, long price, long quantity);
        void EnqueueTradingSignal1(int securityId, long value1, long value2, long value3, long value4);
        
        // ...
    }
}
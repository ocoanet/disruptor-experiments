namespace DisruptorExperiments.MarketData
{
    public interface IMarketDataConflater
    {
        MarketDataUpdate Detach();
    }
}
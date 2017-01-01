namespace DisruptorExperiments.Misc
{
    public interface IFxRateMap
    {
        NumericValue? GetFxRate(int currencyId);
        void UpdateFxRate(int currencyId, NumericValue value);
    }
}
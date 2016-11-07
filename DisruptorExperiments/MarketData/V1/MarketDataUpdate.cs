namespace DisruptorExperiments.MarketData.V1
{
    public class MarketDataUpdate
    {
        public int SecurityId;
        public long? Bid;
        public long? Ask;
        public long? Last;
        public int UpdateCount;

        public void ApplyTo(MarketDataUpdate other)
        {
            if (Bid != null)
                other.Bid = Bid;

            if (Ask != null)
                other.Ask = Ask;

            if (Last != null)
                other.Last = Last;
        }

        public void Reset()
        {
            SecurityId = 0;
            Bid = null;
            Ask = null;
            Last = null;
            UpdateCount = 0;
        }
    }
}
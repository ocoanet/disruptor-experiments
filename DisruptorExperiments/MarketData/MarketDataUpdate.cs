namespace DisruptorExperiments.MarketData
{
    public class MarketDataUpdate
    {
        public long? Bid;
        public long? Ask;
        public long? Last;
        public int UpdateCount;
        public volatile MarketDataUpdate Next;

        public void Apply(MarketDataUpdate other)
        {
            if (Bid != null)
                other.Bid = Bid;

            if (Ask != null)
                other.Ask = Ask;

            if (Last != null)
                other.Last = Last;

            other.UpdateCount += UpdateCount;
        }

        public MarketDataUpdate MergeLinkedList()
        {
            var update = this;
            var next = update.Next;
            while (next != null)
            {
                update.Apply(next);
                update = next;
                next = update.Next;
            }
            return update;
        }

        public void Reset()
        {
            Bid = null;
            Ask = null;
            Last = null;
            UpdateCount = 0;
            Next = null;
        }
    }
}
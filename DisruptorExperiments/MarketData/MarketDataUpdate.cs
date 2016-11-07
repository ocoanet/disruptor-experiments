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

            other.UpdateCount++;
        }

        public void MergeLinkedList()
        {
            var next = Next;
            while (next != null)
            {
                if (Bid == null)
                    Bid = next.Bid;

                if (Ask == null)
                    Ask = next.Ask;

                if (Last != null)
                    Last = next.Last;

                UpdateCount++;
                next = next.Next;
            }
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
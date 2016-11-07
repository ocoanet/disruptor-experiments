using System.Threading;
using DisruptorExperiments.Engine.X;

namespace DisruptorExperiments.MarketData.V2
{
    /// <summary>
    /// Wait-free, SPSC, with allocation.
    /// </summary>
    public class MarketDataConflater : IMarketDataConflater
    {
        private readonly XEngine _targetEngine;
        private readonly int _securityId;
        private MarketDataUpdate _currentUpdate;

        public MarketDataConflater(XEngine targetEngine, int securityId)
        {
            _targetEngine = targetEngine;
            _securityId = securityId;
        }

        public void AddOrMerge(MarketDataUpdate update)
        {
            var newUpdate = new MarketDataUpdate();
            update.Apply(newUpdate);

            var currentUpdate = Volatile.Read(ref _currentUpdate);
            if (currentUpdate != null)
            {
                newUpdate.Next = currentUpdate;
                if (Interlocked.CompareExchange(ref _currentUpdate, newUpdate, currentUpdate) == currentUpdate)
                    return;
            }

            newUpdate.Next = null;
            using (var acquire = _targetEngine.AcquireEvent())
            {
                var currentEvent = acquire.Event;
                currentEvent.SetMarketDataUpdate(_securityId, this);
            }
        }

        public MarketDataUpdate Detach()
        {
            var marketDataUpdate = Interlocked.Exchange(ref _currentUpdate, null);
            marketDataUpdate.MergeLinkedList();
            return marketDataUpdate;
        }
    }
}
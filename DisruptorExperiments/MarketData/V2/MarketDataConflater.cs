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
        private XEvent _event;

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
            Volatile.Write(ref _currentUpdate, newUpdate);

            using (var acquire = _targetEngine.AcquireEvent())
            {
                _event = acquire.Event;
                _event.SetMarketData(_securityId, this);
            }
        }

        public MarketDataUpdate Detach()
        {
            var evt = _event;
            var update = Interlocked.Exchange(ref _currentUpdate, null);

            var mergedUpdate = update.MergeLinkedList();
            mergedUpdate.Apply(evt.MarketDataUpdate);
            return evt.MarketDataUpdate;
        }
    }
}
using DisruptorExperiments.Engine.X;

namespace DisruptorExperiments.MarketData.V1
{
    /// <summary>
    /// Locked-based.
    /// </summary>
    public class MarketDataConflater : IMarketDataConflater
    {
        private readonly XEngine _targetEngine;
        private readonly int _securityId;
        private XEvent _currentEvent;

        public MarketDataConflater(XEngine targetEngine, int securityId)
        {
            _targetEngine = targetEngine;
            _securityId = securityId;
        }

        public void AddOrMerge(MarketDataUpdate update)
        {
            lock (_targetEngine)
            {
                if (_currentEvent == null)
                {
                    using (var acquire = _targetEngine.AcquireEvent())
                    {
                        _currentEvent = acquire.Event;
                        _currentEvent.SetMarketDataUpdate(_securityId, this);
                    }
                }
                update.Apply(_currentEvent.MarketDataUpdate);
            }
        }

        public MarketDataUpdate Detach()
        {
            var currentEvent = _currentEvent;
            lock (_targetEngine)
            {
                _currentEvent = null;
            }
            return currentEvent.MarketDataUpdate;
        }
    }
}
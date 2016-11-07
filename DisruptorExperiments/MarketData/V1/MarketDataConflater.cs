using DisruptorExperiments.Engine.X;

namespace DisruptorExperiments.MarketData.V1
{
    public class MarketDataConflater
    {
        private readonly XEngine _targetEngine;
        private XEvent _currentEvent;

        public MarketDataConflater(XEngine targetEngine)
        {
            _targetEngine = targetEngine;
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
                    }
                }
                update.ApplyTo(_currentEvent.MarketDataUpdate);
            }
        }

        public void Detach()
        {
            lock (_targetEngine)
            {
                _currentEvent = null;
            }
        }
    }
}
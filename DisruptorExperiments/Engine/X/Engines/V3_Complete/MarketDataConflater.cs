using System.Threading;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments.Engine.X.Engines.V3_Complete
{
    /// <summary>
    /// Locked-based.
    /// </summary>
    public class MarketDataConflater
    {
        private SpinLock _spinLock = new SpinLock();
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
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (_currentEvent == null)
                {
                    using (var acquire = _targetEngine.AcquireEvent())
                    {
                        _currentEvent = acquire.Event;
                        _currentEvent.SetMarketData(_securityId, this);
                    }
                }
                update.ApplyTo(_currentEvent.MarketDataUpdate);
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        public MarketDataUpdate Detach()
        {
            var currentEvent = _currentEvent;
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                _currentEvent = null;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
            return currentEvent.MarketDataUpdate;
        }
    }
}
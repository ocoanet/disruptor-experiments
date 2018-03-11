using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Disruptor;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments.Engine.X.Engines.V1_SyncBasedConflation
{
    /// <summary>
    /// Locked-based.
    /// </summary>
    public class MarketDataConflater
    {
        private SpinLock _spinLock = new SpinLock();
        private readonly XEngine _engine;
        private readonly int _securityId;
        private XEvent _currentEvent;

        public MarketDataConflater(XEngine engine, int securityId)
        {
            _engine = engine;
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
                    Add(update);
                }
                else
                {
                    Merge(update);
                }
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(MarketDataUpdate update)
        {
            using (var acquire = _engine.AcquireEvent())
            {
                _currentEvent = acquire.Event;
                _currentEvent.SetMarketData(_securityId, this, update);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Merge(MarketDataUpdate update)
        {
            _currentEvent.EventData.MarketData.MergeWith(update);
        }

        public void Detach()
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
        }
    }
}
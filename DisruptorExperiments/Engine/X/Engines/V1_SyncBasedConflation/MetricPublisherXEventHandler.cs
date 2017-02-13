using System;
using Disruptor;
using HdrHistogram;

namespace DisruptorExperiments.Engine.X.Engines.V1_SyncBasedConflation
{
    public class MetricPublisherXEventHandler : IEventHandler<XEvent>, ILifecycleAware
    {
        private readonly LongHistogram _latencyHistogram;
        private readonly IntHistogram _conflactionHistogram;
        private int _updateCount;
        private int _entryWithUpdateCount;

        public MetricPublisherXEventHandler()
        {
            _latencyHistogram = new LongHistogram(TimeStamp.Minutes(1), 3);
            _conflactionHistogram = new IntHistogram(10000, 1);
        }

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            if (data.EventType != XEventType.MarketData)
                return;

            //_latencyHistogram.RecordValue(data.HandlerMetrics[0].BeginTimestamp - data.AcquireTimestamp);
            //_latencyHistogram.RecordValue(data.HandlerEndTimestamps[0] - data.HandlerBeginTimestamps[0]);
            _conflactionHistogram.RecordValue(data.MarketDataUpdate.UpdateCount);
            _updateCount += data.MarketDataUpdate.UpdateCount;
            _entryWithUpdateCount++;
        }

        public void OnStart()
        {
        }

        public void OnShutdown()
        {
            //_latencyHistogram.OutputPercentileDistribution(Console.Out, outputValueUnitScalingRatio: OutputScalingFactor.TimeStampToMicroseconds, percentileTicksPerHalfDistance: 1);
            _conflactionHistogram.OutputPercentileDistribution(Console.Out, percentileTicksPerHalfDistance: 1);
            Console.WriteLine($"Reveiced UpdateCount: {_updateCount}");
            Console.WriteLine($"Reveiced EntryCount: {_entryWithUpdateCount}");

        }
    }
}
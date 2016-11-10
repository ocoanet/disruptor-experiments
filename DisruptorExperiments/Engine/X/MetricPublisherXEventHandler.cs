using System;
using Disruptor;
using HdrHistogram;

namespace DisruptorExperiments.Engine.X
{
    public class MetricPublisherXEventHandler : IEventHandler<XEvent>, ILifecycleAware
    {
        private readonly LongHistogram _latencyHistogram;
        private readonly IntHistogram _conflactionHistogram;

        public MetricPublisherXEventHandler()
        {
            _latencyHistogram = new LongHistogram(TimeStamp.Minutes(1), 3);
            _conflactionHistogram = new IntHistogram(1000, 1);
        }

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            if (data.EventType != XEventType.MarketDataUpdate)
                return;

            _latencyHistogram.RecordValue(data.HandlerEndTimestamps[0] - data.AcquireTimestamp);
            _conflactionHistogram.RecordValue(data.MarketDataUpdate.UpdateCount);
        }

        public void OnStart()
        {
        }

        public void OnShutdown()
        {
            _latencyHistogram.OutputPercentileDistribution(Console.Out, outputValueUnitScalingRatio: OutputScalingFactor.TimeStampToMicroseconds, percentileTicksPerHalfDistance: 1);
            _conflactionHistogram.OutputPercentileDistribution(Console.Out, percentileTicksPerHalfDistance: 1);
        }
    }
}
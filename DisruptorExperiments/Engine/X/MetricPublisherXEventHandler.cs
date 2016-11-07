using System;
using Disruptor;
using HdrHistogram;

namespace DisruptorExperiments.Engine.X
{
    public class MetricPublisherXEventHandler : IEventHandler<XEvent>, ILifecycleAware
    {
        private readonly LongHistogram _histogram;

        public MetricPublisherXEventHandler()
        {
            _histogram = new LongHistogram(TimeStamp.Hours(1), 3);
        }

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            _histogram.RecordValue(data.HandlerEndTimestamps[0] - data.AcquireTimestamp);
        }

        public void OnStart()
        {
        }

        public void OnShutdown()
        {
            _histogram.OutputPercentileDistribution(Console.Out, outputValueUnitScalingRatio: OutputScalingFactor.TimeStampToMicroseconds, percentileTicksPerHalfDistance: 10);
        }
    }
}
using System;
using System.Diagnostics;
using Disruptor;
using HdrHistogram;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class MetricPublisherXEventHandler : IEventHandler<XEvent>, ILifecycleAware
    {
        private readonly LongHistogram _latencyHistogram = new LongHistogram(TimeStamp.Minutes(1), 3);
        private readonly int _entrySize;

        public MetricPublisherXEventHandler(int entrySize)
        {
            _entrySize = entrySize;
        }

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            _latencyHistogram.RecordValue(Stopwatch.GetTimestamp() - data.Timestamp);
        }

        public void OnStart()
        {
        }

        public void OnShutdown()
        {
            var p50 = _latencyHistogram.GetValueAtPercentile(50) / OutputScalingFactor.TimeStampToMicroseconds;
            var p90 = _latencyHistogram.GetValueAtPercentile(90) / OutputScalingFactor.TimeStampToMicroseconds;
            var p99 = _latencyHistogram.GetValueAtPercentile(99) / OutputScalingFactor.TimeStampToMicroseconds;

            Console.WriteLine(FormattableString.Invariant($"{_entrySize},{p50:0.000},{p90:0.000},{p99:0.000}"));
        }
    }
}
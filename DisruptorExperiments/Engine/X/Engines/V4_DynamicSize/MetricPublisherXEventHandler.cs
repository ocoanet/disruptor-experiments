using System;
using System.Diagnostics;
using System.IO;
using Disruptor;
using HdrHistogram;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class MetricPublisherXEventHandler : IEventHandler<XEvent>, ILifecycleAware
    {
        private readonly LongHistogram _latencyHistogram = new LongHistogram(TimeStamp.Minutes(1), 3);
        private readonly int _entrySize;
        private long _start;

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
            _start = Stopwatch.GetTimestamp();
        }

        public void OnShutdown()
        {
            var stop = Stopwatch.GetTimestamp();
            var elapsed = (stop - _start) / OutputScalingFactor.TimeStampToMilliseconds;

            var p50 = _latencyHistogram.GetValueAtPercentile(50) / OutputScalingFactor.TimeStampToMicroseconds;
            var p90 = _latencyHistogram.GetValueAtPercentile(90) / OutputScalingFactor.TimeStampToMicroseconds;
            var p99 = _latencyHistogram.GetValueAtPercentile(99) / OutputScalingFactor.TimeStampToMicroseconds;

            var latencies = FormattableString.Invariant($"{_entrySize},{p50:0.000},{p90:0.000},{p99:0.000},{elapsed:0}{Environment.NewLine}");
            Console.Write(latencies);

            File.AppendAllText("Latencies.txt", latencies);
        }
    }
}
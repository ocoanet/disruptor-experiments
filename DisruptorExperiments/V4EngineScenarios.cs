using System;
using System.Diagnostics;
using DisruptorExperiments.Engine.X.Engines.V4_DynamicSize;

namespace DisruptorExperiments
{
    public static class V4EngineScenarios
    {
        public static void MeasureLatencyForMultipleSizes()
        {
            foreach (var size in new[] { 1, 1, 2, 5, 10, 20, 50, 100, 120, 150 })
            {
                for (var i = 0; i < 3; i++)
                {
                    MeasureLatency(size);
                    GC.Collect();
                }
            }
        }

        private static void MeasureLatency(int blockCount)
        {
            var entrySize = blockCount * XEvent.BlockSize;

            var engine = new XEngine(entrySize);
            engine.Start();

            var beginIndex = 0;
            byte sequence = 0;
            for (var i = 0; i < 10 * 1000 * 1000; i++)
            {
                var endIndex = beginIndex + XEvent.BlockSize - 1;

                using (var acquireScope = engine.AcquireEvent())
                {
                    var evt = acquireScope.Event;
                    for (var dataIndex = beginIndex; dataIndex <= endIndex; dataIndex++)
                    {
                        evt.Data[dataIndex] = sequence++;
                    }
                    evt.BeginOffset = beginIndex;
                    evt.EndOffset = endIndex;
                    evt.Timestamp = Stopwatch.GetTimestamp();
                }

                beginIndex = (beginIndex + XEvent.BlockSize) % entrySize;
            }

            engine.Stop();
            engine.Dispose();
        }
    }
}

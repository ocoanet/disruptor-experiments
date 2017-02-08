using System;
using System.Diagnostics;
using DisruptorExperiments.Engine.X.Engines.V4_DynamicSize;

namespace DisruptorExperiments
{
    public static class V4EngineScenarios
    {
        public static void MeasureLatency(int blockCount)
        {
            const int blockSize = 4 * sizeof(long);
            var entrySize = blockCount * blockSize;

            var engine = new XEngine(entrySize);
            engine.Start();

            var beginIndex = 0;
            byte b = 0;
            for (var i = 0; i < 10 * 1000 * 1000; i++)
            {
                var endIndex = beginIndex + blockSize - 1;

                using (var acquireScope = engine.AcquireEvent())
                {
                    var evt = acquireScope.Event;
                    for (int dataIndex = beginIndex; dataIndex <= endIndex; dataIndex++)
                    {
                        evt.Data[dataIndex] = b++;
                    }
                    evt.BeginOffset = beginIndex;
                    evt.EndOffset = endIndex;
                    evt.Timestamp = Stopwatch.GetTimestamp();
                }

                beginIndex = (beginIndex + blockSize) % entrySize;
            }

            engine.Stop();
        }
    }
}

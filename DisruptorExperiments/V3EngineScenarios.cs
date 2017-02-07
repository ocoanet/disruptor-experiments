using System;
using System.Diagnostics;
using System.Threading;
using DisruptorExperiments.Engine.X.Engines.V3_Complete;

namespace DisruptorExperiments
{
    public static class V3EngineScenarios
    {
        public static void MeasureEnqueue()
        {
            var engine = new XEngine();
            engine.Start();

            var stopwatch = new Stopwatch();

            for (int i = 0; i < 2; i++)
            {
                stopwatch.Restart();
                PublishTradingSignalV1(engine);
                stopwatch.Stop();
                var v1 = stopwatch.Elapsed;
                Console.WriteLine($"V1: {v1}");
                Thread.Sleep(200);

                stopwatch.Restart();
                PublishTradingSignalV2(engine);
                stopwatch.Stop();
                var v2 = stopwatch.Elapsed;
                Console.WriteLine($"V2: {v2}");
                Console.WriteLine($"Delta: {(v2.Ticks - v1.Ticks) / (double)v1.Ticks:P2}");
                Thread.Sleep(200);
            }

            engine.Stop();
        }

        private static void PublishTradingSignalV1(Engine.X.Interfaces.V1_MethodPerEventType.IXEngine engine)
        {
            for (int i = 0; i < 15000000; i++)
            {
                engine.EnqueueTradingSignal1(i, 1000, 500, 1, 42);
                Thread.SpinWait(1 << 4);
            }
        }

        private static void PublishTradingSignalV2(Engine.X.Interfaces.V2_ExposeEvent.IXEngine engine)
        {
            for (int i = 0; i < 15000000; i++)
            {
                using (var acquiredEvent = engine.AcquireEvent())
                {
                    acquiredEvent.Event.SetTradingSignal1(i, 1000, 500, 1, 42);
                }
                Thread.SpinWait(1 << 4);
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DisruptorExperiments.Engine.X;
using DisruptorExperiments.MarketData;
using DisruptorExperiments.MarketData.V2;

namespace DisruptorExperiments
{
    class Program
    {
        public static void Main(string[] args)
        {
            var engine = new XEngine();
            engine.Start();

            //var publisher = new MarketDataPublisher(engine, securityCount: 50);
            //publisher.Run(TimeSpan.FromSeconds(10));
            //Console.WriteLine($"Generated UpdateCount: {publisher.UpdateCount}");

            var stopwatch = new Stopwatch();

            for (int i = 0; i < 2; i++)
            {
                stopwatch.Restart();
                PublishTradingSignalV1(engine);
                stopwatch.Stop();
                Console.WriteLine($"V1: {stopwatch.Elapsed}");
                Thread.Sleep(200);

                stopwatch.Restart();
                PublishTradingSignalV2(engine);
                stopwatch.Stop();
                Console.WriteLine($"V2: {stopwatch.Elapsed}");
                Thread.Sleep(200);

                stopwatch.Restart();
                PublishTradingSignalV3(engine);
                stopwatch.Stop();
                Console.WriteLine($"V3: {stopwatch.Elapsed}");
                Thread.Sleep(200);
            }

            engine.Stop();

            Console.WriteLine("...");
            Console.ReadKey();
        }

        private static void PublishTradingSignalV1(Engine.X.Interfaces.V1.IXEngine engine)
        {
            for (int i = 0; i < 5000000; i++)
            {
                engine.EnqueueTradingSignal1(i, 1000, 500, 1, 42);
                Thread.SpinWait(1 << 4);
            }
        }

        private static void PublishTradingSignalV2(Engine.X.Interfaces.V2.IXEngine engine)
        {
            for (int i = 0; i < 5000000; i++)
            {
                using (var acquiredEvent = engine.AcquireEvent())
                {
                    acquiredEvent.Event.SetTradingSignal1(i, 1000, 500, 1, 42);
                }
                Thread.SpinWait(1 << 4);
            }
        }

        private static void PublishTradingSignalV3(Engine.X.Interfaces.V3.IXEngine engine)
        {
            for (int i = 0; i < 5000000; i++)
            {
                using (var acquiredEvent = engine.AcquireEventRef())
                {
                    acquiredEvent.Event.SetTradingSignal1(i, 1000, 500, 1, 42);
                }
                Thread.SpinWait(1 << 4);
            }
        }

        private class MarketDataPublisher
        {
            private readonly Random _random = new Random();
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly MarketDataUpdate _marketDataUpdate = new MarketDataUpdate { UpdateCount = 1 };
            private readonly long[] _prices;
            private readonly MarketDataConflater[] _conflacters;

            public MarketDataPublisher(XEngine targetEngine, int securityCount)
            {
                _prices = Enumerable.Repeat(500L, securityCount).ToArray();
                _conflacters = Enumerable.Range(0, securityCount).Select(x => new MarketDataConflater(targetEngine, x)).ToArray();
            }

            public int UpdateCount { get; private set; }

            public void Run(TimeSpan duration)
            {
                var collectionCountBefore = GC.CollectionCount(0);

                _stopwatch.Restart();
                while (_stopwatch.Elapsed < duration)
                {
                    var securityId = _random.Next(_prices.Length);
                    _prices[securityId] += 5 - _random.Next(10);

                    _marketDataUpdate.Last = _prices[securityId];
                    _conflacters[securityId].AddOrMerge(_marketDataUpdate);

                    UpdateCount++;
                    Thread.SpinWait(1 + _random.Next(1 << 5));
                }

                var collectionCount = GC.CollectionCount(0) - collectionCountBefore;
                Console.WriteLine($"CollectionCount: {collectionCount}");
            }
        }
    }
}

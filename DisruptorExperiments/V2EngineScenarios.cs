using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DisruptorExperiments.Engine.X.Engines.V2_BatchBasedConflation;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments
{
    public static class V2EngineScenarios
    {
        public static void PublisherMarketDataUpdates()
        {
            var engine = new XEngine();
            engine.Start();

            var publisher = new MarketDataPublisher2(engine, securityCount: 50);
            publisher.Run(1_000_000);
            Console.WriteLine($"Generated UpdateCount: {publisher.UpdateCount}");

            engine.Stop();
        }

        private class MarketDataPublisher2
        {
            private readonly Random _random = new Random();
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly MarketDataUpdate _marketDataUpdate = new MarketDataUpdate { UpdateCount = 1 };
            private readonly XEngine _targetEngine;
            private readonly long[] _prices;

            public MarketDataPublisher2(XEngine targetEngine, int securityCount)
            {
                _targetEngine = targetEngine;
                _prices = Enumerable.Repeat(500L, securityCount).ToArray();
            }

            public int UpdateCount { get; private set; }

            public void Run(int entryCount)
            {
                var collectionCountBefore = GC.CollectionCount(0);

                _stopwatch.Restart();

                for (int i = 0; i < entryCount; i++)
                {
                    var securityId = _random.Next(_prices.Length);
                    _prices[securityId] += 5 - _random.Next(10);

                    _marketDataUpdate.Last = _prices[securityId];

                    using (var acquire = _targetEngine.AcquireEvent())
                    {
                        acquire.Event.SetMarketData(securityId, _marketDataUpdate);
                    }

                    UpdateCount++;
                    Thread.SpinWait(1 + _random.Next(1 << 5));
                }

                _stopwatch.Stop();
                Console.WriteLine($"Elapsed: {_stopwatch.Elapsed}");

                var collectionCount = GC.CollectionCount(0) - collectionCountBefore;
                Console.WriteLine($"CollectionCount: {collectionCount}");
            }
        }
    }
}
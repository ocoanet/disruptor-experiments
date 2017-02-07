using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DisruptorExperiments.Engine.X.Engines.V1_SyncBasedConflaction;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments
{
    public static class V1EngineScenarios
    {
        public static void PublisherMarketDataUpdates()
        {
            var engine = new XEngine();
            engine.Start();

            var publisher = new MarketDataPublisher1(engine, securityCount: 50);
            publisher.Run(TimeSpan.FromSeconds(10));
            Console.WriteLine($"Generated UpdateCount: {publisher.UpdateCount}");

            engine.Stop();
        }

        private class MarketDataPublisher1
        {
            private readonly Random _random = new Random();
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly MarketDataUpdate _marketDataUpdate = new MarketDataUpdate { UpdateCount = 1 };
            private readonly Dictionary<int, MarketDataConflater> _conflacters;
            private readonly long[] _prices;

            public MarketDataPublisher1(XEngine targetEngine, int securityCount)
            {
                _conflacters = Enumerable.Range(0, securityCount).ToDictionary(x => x, x => new MarketDataConflater(targetEngine, x));
                _prices = Enumerable.Repeat(500L, securityCount).ToArray();
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
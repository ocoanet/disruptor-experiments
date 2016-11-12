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
        static void Main(string[] args)
        {
            var engine = new XEngine();
            engine.Start();

            var publisher = new MarketDataPublisher(engine, securityCount: 50);

            var collectionCountBefore = GC.CollectionCount(0);

            publisher.Run(TimeSpan.FromSeconds(10));

            var collectionCount = GC.CollectionCount(0) - collectionCountBefore;
            Console.WriteLine($"CollectionCount: {collectionCount}");

            engine.Stop();

            Console.WriteLine($"Generated UpdateCount: {publisher.UpdateCount}");

            Console.WriteLine("...");
            Console.ReadKey();
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
            }
        }
    }
}

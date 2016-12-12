using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DisruptorExperiments.Engine.X;
using DisruptorExperiments.Engine.X.Engines.V3_Complete;
using DisruptorExperiments.Engine.X.Interfaces.V1_MethodPerEventType;
using DisruptorExperiments.MarketData;

namespace DisruptorExperiments
{
    class Program
    {
        public static void Main(string[] args)
        {
            var engine = new XEngine();
            engine.Start();

            //var publisher = new MarketDataPublisher1(engine, securityCount: 50);
            //publisher.Run(TimeSpan.FromSeconds(10));
            //Console.WriteLine($"Generated UpdateCount: {publisher.UpdateCount}");

            var publisher = new MarketDataPublisher2(engine, securityCount: 50);
            publisher.Run(TimeSpan.FromSeconds(10));
            Console.WriteLine($"Generated UpdateCount: {publisher.UpdateCount}");

            //MeasureEnqueue(engine);

            engine.Stop();

            Console.WriteLine("...");
            Console.ReadKey();
        }

        private static void MeasureEnqueue(XEngine engine)
        {
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
        }

        private static void PublishTradingSignalV1(IXEngine engine)
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

            public void Run(TimeSpan duration)
            {
                var collectionCountBefore = GC.CollectionCount(0);

                _stopwatch.Restart();
                while (_stopwatch.Elapsed < duration)
                {
                    var securityId = _random.Next(_prices.Length);
                    _prices[securityId] += 5 - _random.Next(10);

                    _marketDataUpdate.Last = _prices[securityId];

                    using (var acquire = _targetEngine.AcquireEvent())
                    {
                        acquire.Event.SetMarketDataForBatching(securityId, _marketDataUpdate);
                    }

                    UpdateCount++;
                    Thread.SpinWait(1 + _random.Next(1 << 5));
                }

                var collectionCount = GC.CollectionCount(0) - collectionCountBefore;
                Console.WriteLine($"CollectionCount: {collectionCount}");
            }
        }
    }
}

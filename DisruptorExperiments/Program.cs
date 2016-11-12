using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DisruptorExperiments.Engine.X;
using DisruptorExperiments.MarketData;
using DisruptorExperiments.MarketData.V1;

namespace DisruptorExperiments
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new XEngine();
            engine.Start();

            PublishMarketDataUpdates(engine);

            Thread.Sleep(1000);

            engine.Stop();

            Console.WriteLine("...");
            Console.ReadKey();
        }

        private static void PublishMarketDataUpdates(XEngine engine)
        {
            const int securityCount = 50;

            var random = new Random();
            var prices = Enumerable.Repeat(500L, securityCount).ToArray();
            var conflacters = Enumerable.Range(0, securityCount).Select(x => new MarketDataConflater(engine, x)).ToArray();
            var marketDataUpdate = new MarketDataUpdate { UpdateCount = 1 };
            var updateCount = 0;

            var duration = Stopwatch.StartNew();
            while (duration.Elapsed < TimeSpan.FromSeconds(10))
            {
                var securityId = random.Next(prices.Length);
                prices[securityId] += 5 - random.Next(10);

                marketDataUpdate.Last = prices[securityId];
                conflacters[securityId].AddOrMerge(marketDataUpdate);

                updateCount++;
                Thread.SpinWait(1 + random.Next(1 << 5));
            }

            Console.WriteLine($"Generated UpdateCount: {updateCount}");
        }
    }
}

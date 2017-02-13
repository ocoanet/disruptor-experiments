using System;

namespace DisruptorExperiments
{
    public static class Program
    {
        public static void Main()
        {
            V4EngineScenarios.MeasureLatencyForMultipleSizes();

            Console.WriteLine("...");
            Console.ReadKey();
        }
    }
}

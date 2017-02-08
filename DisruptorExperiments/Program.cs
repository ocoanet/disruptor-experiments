using System;

namespace DisruptorExperiments
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            V4EngineScenarios.MeasureLatency(1);

            Console.WriteLine("...");
            Console.ReadKey();
        }

        
    }
}

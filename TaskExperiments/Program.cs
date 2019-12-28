using System;
using System.Threading.Tasks;
using Disruptor.Dsl;
using TaskExperiments.Publisher.Custom;

namespace TaskExperiments
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var disruptor = new Disruptor<SampleEvent>(() => new SampleEvent(), 1024, TaskScheduler.Default);

            disruptor.HandleEventsWith(new SampleEventHandler())
                .Then(new AwaitableEventContinuationInvoker<SampleEvent>());

            disruptor.Start();

            var s = disruptor.RingBuffer.Next();
            disruptor.RingBuffer[s].Id = 42;
            await disruptor.RingBuffer.PublishAsync(s);

            var allocatedBytesBefore = GC.GetTotalAllocatedBytes(true);

            for (int i = 0; i < 1_000_000; i++)
            {
                var sequence = disruptor.RingBuffer.Next();
                disruptor.RingBuffer[sequence].Id = 42;
                await disruptor.RingBuffer.PublishAsync(sequence);
            }
            
            var allocatedBytesAfter = GC.GetTotalAllocatedBytes(true);

            Console.WriteLine(allocatedBytesAfter - allocatedBytesBefore);
            Console.WriteLine("A");

            await Task.Yield();

            disruptor.Shutdown();

            Console.WriteLine("B");
            Console.ReadLine();
        }
    }
}
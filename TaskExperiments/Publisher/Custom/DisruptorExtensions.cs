using Disruptor;
using Disruptor.Dsl;

namespace TaskExperiments.Publisher.Custom
{
    public static class DisruptorExtensions
    {
        public static EventTask PublishAsync<T>(this Disruptor<T> disruptor, long sequence)
            where T : AwaitableEvent
        {
            return disruptor.RingBuffer.PublishAsync(sequence);
        }
        
        public static EventTask PublishAsync<T>(this RingBuffer<T> ringBuffer, long sequence)
            where T : AwaitableEvent
        {
            var eventTask = new EventTask(ringBuffer[sequence], sequence);
            
            ringBuffer.Publish(sequence);
            
            return eventTask;
        }
    }
}

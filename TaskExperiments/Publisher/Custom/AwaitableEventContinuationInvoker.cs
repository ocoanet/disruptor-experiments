using Disruptor;

namespace TaskExperiments.Publisher.Custom
{
    public class AwaitableEventContinuationInvoker<T> : IEventHandler<T>
        where T : AwaitableEvent
    {
        public void OnEvent(T data, long sequence, bool endOfBatch)
        {
            data.SignalCompletion(sequence);
        }
    }
}
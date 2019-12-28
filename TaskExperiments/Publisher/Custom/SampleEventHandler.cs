using System.Threading;
using Disruptor;

namespace TaskExperiments.Publisher.Custom
{
    public class SampleEventHandler : IEventHandler<SampleEvent>
    {
        public void OnEvent(SampleEvent data, long sequence, bool endOfBatch)
        {
            Thread.SpinWait(1 << 5);
        }
    }
}
using Disruptor;

namespace DisruptorExperiments.Engine.X
{
    public class DumperXEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
        }
    }
}
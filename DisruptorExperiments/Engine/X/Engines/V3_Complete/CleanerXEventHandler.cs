using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V3_Complete
{
    public class CleanerXEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            data.Reset();
        }
    }
}
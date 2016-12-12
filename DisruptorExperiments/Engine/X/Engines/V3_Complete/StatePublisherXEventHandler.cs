using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V3_Complete
{
    public class StatePublisherXEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
        }
    }
}
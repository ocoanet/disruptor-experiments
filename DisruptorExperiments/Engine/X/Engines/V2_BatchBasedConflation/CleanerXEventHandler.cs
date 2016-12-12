using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V2_BatchBasedConflation
{
    public class CleanerXEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            data.Reset();
        }
    }
}
using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class CleanerXEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            for (var index = data.BeginOffset; index <= data.EndOffset; index++)
            {
                data.Data[index] = 0;
            }
        }
    }
}
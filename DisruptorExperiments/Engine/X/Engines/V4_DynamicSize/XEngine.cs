using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class XEngine
    {
        private readonly Disruptor<XEvent> _disrutpor;
        private readonly RingBuffer<XEvent> _ringBuffer;

        public XEngine(int entrySize)
        {
            _disrutpor = new Disruptor<XEvent>(() => new XEvent(entrySize), 32768, TaskScheduler.Default);
            _disrutpor.HandleEventsWith(new Business1XEventHandler())
                .Then(new Business2XEventHandler())
                .Then(new MetricPublisherXEventHandler(entrySize))
                .Then(new CleanerXEventHandler())
                ;

            _ringBuffer = _disrutpor.RingBuffer;
        }

        public AcquireScope<XEvent> AcquireEvent()
        {
            var sequence = _ringBuffer.Next();
            var data = _ringBuffer[sequence];

            return new AcquireScope<XEvent>(_ringBuffer, sequence, data);
        }

        public void Start()
        {
            _disrutpor.Start();
        }

        public void Stop()
        {
            _disrutpor.Shutdown();
        }
    }
}
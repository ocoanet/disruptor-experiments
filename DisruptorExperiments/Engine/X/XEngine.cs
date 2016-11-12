using System.Threading.Tasks;
using Disruptor.Dsl;

namespace DisruptorExperiments.Engine.X
{
    public class XEngine : IXEngine
    {
        private readonly Disruptor<XEvent> _disrutpor;

        public XEngine()
        {
            _disrutpor = new Disruptor<XEvent>(() => new XEvent(), 4096, TaskScheduler.Default);
            _disrutpor.HandleEventsWith(new BusinessXEventHandler())
                .Then(new StatePublisherXEventHandler())
                .Then(new DumperXEventHandler())
                .Then(new MetricPublisherXEventHandler())
                .Then(new CleanerXEventHandler())
                ;
        }

        public AcquireScope<XEvent> AcquireEvent()
        {
            var sequence = _disrutpor.RingBuffer.Next();
            _disrutpor.RingBuffer[sequence].OnAcquired();
            return new AcquireScope<XEvent>(_disrutpor.RingBuffer, sequence);
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
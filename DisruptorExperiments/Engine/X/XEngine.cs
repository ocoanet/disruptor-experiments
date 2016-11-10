using System;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace DisruptorExperiments.Engine.X
{
    public class XEngine
    {
        private readonly Disruptor<XEvent> _disrutpor;

        public XEngine()
        {
            _disrutpor = new Disruptor<XEvent>(() => new XEvent(), 1024, TaskScheduler.Default);
            _disrutpor.HandleEventsWith(new BusinessXEventHandler())
                //.Then(new StatePublisherXEventHandler())
                //.Then(new DumperXEventHandler())
                .Then(new MetricPublisherXEventHandler())
                .Then(new CleanerXEventHandler())
                ;
        }

        public AcquireScope AcquireEvent()
        {
            return new AcquireScope(_disrutpor.RingBuffer, _disrutpor.RingBuffer.Next());
        }

        public void Start()
        {
            _disrutpor.Start();
        }

        public void Stop()
        {
            _disrutpor.Shutdown();
        }

        public struct AcquireScope : IDisposable
        {
            private RingBuffer<XEvent> _ringBuffer;
            private long _sequence;

            public AcquireScope(RingBuffer<XEvent> ringBuffer, long sequence)
            {
                _ringBuffer = ringBuffer;
                _sequence = sequence;

                Event.OnAcquired();
            }

            public XEvent Event { get { return _ringBuffer[_sequence]; } }

            public void Dispose()
            {
                _ringBuffer.Publish(_sequence);
            }
        }
    }
}
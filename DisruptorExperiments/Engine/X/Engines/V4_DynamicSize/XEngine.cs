using System;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using DisruptorExperiments.Misc;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class XEngine : IDisposable
    {
        private readonly TaskScheduler _taskScheduler = new ExperimentalTaskScheduler(4, 1, 3, 5, 7);
        private readonly Disruptor<XEvent> _disrutpor;
        private readonly RingBuffer<XEvent> _ringBuffer;

        public XEngine(int entrySize)
        {
            // 16384, 32768, 65536
            _disrutpor = new Disruptor<XEvent>(() => new XEvent(entrySize), 32768, _taskScheduler, ProducerType.Single, new BusySpinWaitStrategy());
            _disrutpor.HandleEventsWith(new BusinessXEventHandler())
                      .Then(new BusinessXEventHandler())
                      .Then(new MetricPublisherXEventHandler(entrySize))
                      .Then(new CleanerXEventHandler());

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

        public void Dispose()
        {
            (_taskScheduler as IDisposable)?.Dispose();
        }
    }
}
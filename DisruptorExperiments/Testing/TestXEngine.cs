using DisruptorExperiments.Engine.X;
using DisruptorExperiments.Engine.X.Interfaces.V3;

namespace DisruptorExperiments.Testing
{
    public class TestXEngine : IXEngine
    {
        public AcquireScope<XEvent> AcquireEventRef()
        {
            var acquireScope = new AcquireScope<XEvent>(new XEvent(5));
            // WIP
            return acquireScope;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
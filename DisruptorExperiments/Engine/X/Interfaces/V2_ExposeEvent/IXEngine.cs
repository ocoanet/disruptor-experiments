using DisruptorExperiments.Engine.X.Engines.V3_Complete;

namespace DisruptorExperiments.Engine.X.Interfaces.V2_ExposeEvent
{
    public interface IXEngine
    {
        AcquireScope<XEvent> AcquireEvent();

        void Start();
        void Stop();
    }
}
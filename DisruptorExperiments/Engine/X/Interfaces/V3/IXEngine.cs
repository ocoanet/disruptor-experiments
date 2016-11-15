namespace DisruptorExperiments.Engine.X.Interfaces.V3
{
    public interface IXEngine
    {
        AcquireScope<XEvent> AcquireEventRef();

        void Start();
        void Stop();
    }
}
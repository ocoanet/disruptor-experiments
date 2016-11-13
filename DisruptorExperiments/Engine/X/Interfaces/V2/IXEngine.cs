namespace DisruptorExperiments.Engine.X.Interfaces.V2
{
    public interface IXEngine
    {
        AcquireScope<XEvent> AcquireEvent();

        void Start();
        void Stop();
    }
}
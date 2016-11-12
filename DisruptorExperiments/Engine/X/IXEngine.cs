namespace DisruptorExperiments.Engine.X
{
    public interface IXEngine
    {
        AcquireScope<XEvent> AcquireEvent();

        void Start();
        void Stop();
    }
}
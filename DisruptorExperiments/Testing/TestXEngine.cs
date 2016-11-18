using DisruptorExperiments.Engine.X;

namespace DisruptorExperiments.Testing
{
    public class TestXEngine : TestEngine<XEvent>
    {
        public TestXEngine() : base(() => new XEvent(5))
        {
        }
    }
}
using DisruptorExperiments.Engine.X;

namespace DisruptorExperiments.Tests.Engine.X
{
    public class TestXEngine : TestEngine<XEvent>
    {
        public TestXEngine() : base(() => new XEvent(5))
        {
        }
    }
}
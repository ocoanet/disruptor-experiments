using DisruptorExperiments.Engine.X;
using DisruptorExperiments.Engine.X.Engines.V3_Complete;

namespace DisruptorExperiments.Tests.Engine.X
{
    public class TestXEngine : TestEngine<XEvent>
    {
        public TestXEngine() : base(() => new XEvent(5))
        {
        }
    }
}
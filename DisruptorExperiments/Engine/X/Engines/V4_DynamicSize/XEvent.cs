using System.Diagnostics;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class XEvent
    {
        public XEvent(int size)
        {
            Data = new byte[size];
        }

        public long Timestamp;
        public byte[] Data;
        public int Sum1;
        public int Sum2;
        public int BeginOffset;
        public int EndOffset;
    }
}
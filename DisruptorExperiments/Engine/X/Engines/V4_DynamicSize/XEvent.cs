namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class XEvent
    {
        public const int BlockSize = 4 * sizeof(long);

        public XEvent(int size)
        {
            Data = new byte[size];
        }

        public long Timestamp;
        public byte[] Data;
        public int BeginOffset;
        public int EndOffset;
    }
}
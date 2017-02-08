using System;
using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class Business2XEventHandler : IEventHandler<XEvent>
    {
        private readonly Random _random = new Random();

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            var sum = 0;
            for (var index = data.BeginOffset; index < data.EndOffset; index++)
            {
                sum += data.Data[index];
            }
            data.Sum2 = sum + _random.Next();
        }
    }
}
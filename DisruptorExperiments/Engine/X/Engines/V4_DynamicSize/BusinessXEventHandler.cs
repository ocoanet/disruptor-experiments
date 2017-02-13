using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Disruptor;

namespace DisruptorExperiments.Engine.X.Engines.V4_DynamicSize
{
    public class BusinessXEventHandler : IEventHandler<XEvent>, ILifecycleAware
    {
        private readonly Dictionary<byte, State> _states = Enumerable.Range(0, byte.MaxValue + 1)
                                                                     .ToDictionary(x => (byte)x, x => new State());

        private readonly Random _random = new Random();

        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            var state = _states[data.Data[data.BeginOffset]];
            Array.Copy(data.Data, state.Data, XEvent.BlockSize);

            var sum = 0;
            for (var index = data.BeginOffset; index <= data.EndOffset; index++)
            {
                sum += data.Data[index];
            }
            sum += _random.Next();

            Thread.SpinWait(1 << 3);

            state.Sum = sum;
        }

        public void OnStart()
        {
        }

        public void OnShutdown()
        {
            foreach (var state in _states.Values)
            {
                GC.KeepAlive(state.Sum);
            }
        }

        private class State
        {
            public const int DataSize = XEvent.BlockSize + 64;
            public readonly byte[] Data = new byte[DataSize];
            public int Sum;
        }
    }
}
using Disruptor;
using System;

namespace DisruptorExperiments.Engine.X.Events.V1_PropertyMess
{
    public class XEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            switch (data.EventType)
            {
                case XEventType.MarketData:
                    OnMarketData(ref data.MarketData);
                    break;

                case XEventType.Execution:
                    OnExecution(ref data.Execution);
                    break;

                case XEventType.TradingSignal1:
                    break;
                case XEventType.TradingSignal2:
                    break;
                default:
                    break;
            }
        }

        private void OnExecution(ref XEvent.ExecutionInfo execution)
        {
            throw new NotImplementedException();
        }

        private void OnMarketData(ref XEvent.MarketDataInfo marketData)
        {
            throw new NotImplementedException();
        }
    }
}

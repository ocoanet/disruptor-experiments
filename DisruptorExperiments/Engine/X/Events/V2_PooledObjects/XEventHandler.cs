using Disruptor;
using System;

namespace DisruptorExperiments.Engine.X.Events.V2_PooledObjects
{
    public class XEventHandler : IEventHandler<XEvent>
    {
        public void OnEvent(XEvent data, long sequence, bool endOfBatch)
        {
            switch (data.EventType)
            {
                case XEventType.MarketData:
                    var marketDataInfo = (XEvent.MarketDataInfo)data.EventData;
                    OnMarketData(marketDataInfo);
                    break;

                case XEventType.Execution:
                    OnExecution((XEvent.ExecutionInfo)data.EventData);
                    break;

                case XEventType.TradingSignal1:
                    break;
                case XEventType.TradingSignal2:
                    break;
                default:
                    break;
            }
        }

        private void OnExecution(XEvent.ExecutionInfo execution)
        {
            throw new NotImplementedException();
        }

        private void OnMarketData(XEvent.MarketDataInfo marketData)
        {
            throw new NotImplementedException();
        }
    }
}

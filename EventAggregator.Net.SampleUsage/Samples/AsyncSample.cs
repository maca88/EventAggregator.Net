using System.Threading;
using System.Threading.Tasks;
using EventAggregator.Net.SampleUsage;

namespace EventAggregatorNet.SampleUsage.Samples
{
#if ASYNC
    public class AsyncSample
    {
        public static void Run()
        {
            var config = new EventAggregator.Config
            {
                // Make the marshaler run in the background thread
                DefaultThreadAsyncMarshaler = action => Task.Factory.StartNew(action)
            };

            var eventAggregationManager = new EventAggregator(config);
            eventAggregationManager.AddListener(new LongRunningHandler());

            "EventAggregator setup complete".Log();

            eventAggregationManager.SendMessage<SampleEventMessage>();
        }
    }


    public class LongRunningHandler : IListenerAsync<SampleEventMessage>
    {
        public async Task Handle(SampleEventMessage message)
        {
            "LongRunningHandler - Received event".Log();
            await TaskEx.Delay(1000);
            "LongRunningHandler - Done with work".Log();
        }
    }
#endif
}
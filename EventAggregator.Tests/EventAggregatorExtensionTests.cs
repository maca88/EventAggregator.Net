using System.Threading.Tasks;
using Xunit;

namespace EventAggregatorNet.Tests
{
	public class EventAggregatorExtensionTests
	{
		[Fact]
		public void Can_use_delegate_to_subscribe_to_message()
		{
            var eventAggregator = new EventAggregator();
			SomeMessage messageTrapped = null;

			eventAggregator.AddListenerAction<SomeMessage>(msg => { messageTrapped = msg; });
			eventAggregator.SendMessage<SomeMessage>();

			messageTrapped.ShouldNotBeNull();
		}

        [Fact]
		public void Can_use_unsubscribe_from_delegate_handler()
		{
            var eventAggregator = new EventAggregator();
			SomeMessage messageTrapped = null;

			var disposable = eventAggregator.AddListenerAction<SomeMessage>(msg => { messageTrapped = msg; });
			disposable.Dispose();
			eventAggregator.SendMessage<SomeMessage>();

			messageTrapped.ShouldBeNull();
		}

#if ASYNC
        [Fact]
        public async Task Can_use_delegate_to_subscribe_to_message_async()
        {
            var eventAggregator = new EventAggregator();
            SomeMessage messageTrapped = null;

            eventAggregator.AddListenerAction<SomeMessage>(async msg =>
            {
                await Task.Delay(500);
                messageTrapped = msg;
            });
            await eventAggregator.SendMessageAsync<SomeMessage>();

            messageTrapped.ShouldNotBeNull();
        }

        [Fact]
        public async Task Can_use_unsubscribe_from_delegate_handler_async()
        {
            var eventAggregator = new EventAggregator();
            SomeMessage messageTrapped = null;

            var disposable = eventAggregator.AddListenerAction<SomeMessage>(async msg =>
            {
                await Task.Delay(500);
                messageTrapped = msg;
            });
            disposable.Dispose();
            await eventAggregator.SendMessageAsync<SomeMessage>();

            messageTrapped.ShouldBeNull();
        }
#endif
    }

}
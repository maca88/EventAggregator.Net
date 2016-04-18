using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace EventAggregatorNet.Tests
{
    public class EventAggregatorTests
    {
        [Fact]
        public void Should_send_message()
        {
            var someMessageHandler = new SomeMessageHandler();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.SendMessage<SomeMessage>();
            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

#if !UNWRAP_EX
        [Fact]
        public void Should_throws_target_invocation_exception_when_send_sync_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler4();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<TargetInvocationException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

        [Fact]
        public void Should_throws_target_invocation_exception_with_real_inner_exception_when_send_sync_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler5();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            try
            {
                eventAggregator.SendMessage<SomeMessage>();
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsType<RealException>(ex.InnerException);
            }
        }

#else
        [Fact]
        public void Should_throws_invalid_operation_exception_when_send_sync_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler4();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<InvalidOperationException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

        [Fact]
        public void Should_throws_real_exception_when_send_sync_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler5();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<RealException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

#endif

        [Fact]
        public void When_a_listener_has_been_garbage_collected_and_an_event_is_published_the_zombied_handler_should_be_removed()
        {
            var eventAggregator = new EventAggregator();

            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator);
            GC.Collect();

            eventAggregator.SendMessage<SomeMessage>();

            eventAggregator.GetListeners().Count().ShouldEqual(0);
        }

        private void AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(
            IEventSubscriptionManager eventSubscriptionManager, bool? holdStrongReference = false)
        {
            var someMessageHandler = new SomeMessageHandler();
            eventSubscriptionManager.AddListener(someMessageHandler, holdStrongReference);
        }

        [Fact]
        public void When_instructed_to_hold_a_strong_reference_by_default_and_the_listener_is_attempted__been_garbage_collected_and_an_event_is_published_the_zombied_handler_should_be_removed()
        {
            var config = new EventAggregator.Config
            {
                HoldReferences = true
            };
            var eventAggregator = new EventAggregator(config);

            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator, null);
            GC.Collect();

            eventAggregator.SendMessage<SomeMessage>();

            eventAggregator.GetListeners().Count().ShouldEqual(1);
        }

        

        [Fact]
        public void When_instructed_to_hold_a_strong_reference_and_the_listener_is_attempted__been_garbage_collected_and_an_event_is_published_the_zombied_handler_should_be_removed()
        {
            var config = new EventAggregator.Config
            {
                HoldReferences = true
            };
            var eventAggregator = new EventAggregator(config);

            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator, true);
            GC.Collect();

            eventAggregator.SendMessage<SomeMessage>();

            eventAggregator.GetListeners().Count().ShouldEqual(1);
        }

        

        [Fact]
        public void Can_remove_a_good_listener_with_a_zombied_listener()
        {
            var eventAggregator = new EventAggregator();

            SomeMessageHandler2 messageHandler2 = new SomeMessageHandler2();
            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator, false);
            eventAggregator.AddListener(messageHandler2);
            GC.Collect();

            // both good and zombied listeners
            eventAggregator.GetListeners().Count().ShouldEqual(2);

            // should not throw if removing a good listener
            eventAggregator.RemoveListener(messageHandler2);

            // should be only zombie left
            eventAggregator.GetListeners().Count().ShouldEqual(1);
            eventAggregator.SendMessage<SomeMessage>();

            // after call to SendMessage, the zombied listener should be removed.
            eventAggregator.GetListeners().Count().ShouldEqual(0);
        }

        [Fact]
        public void Can_unsubscribe_manually()
        {
            var someMessageHandler = new SomeMessageHandler();
            var eventAggregator = new EventAggregator();
            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.SendMessage<SomeMessage>();
            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);


            eventAggregator.RemoveListener(someMessageHandler);
            eventAggregator.SendMessage<SomeMessage>();

            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }


        [Fact]
        public void When_no_subscribers_can_detect_nothing_was_published()
        {
            var config = new EventAggregator.Config();
            bool warningWasCalled = false;
            config.OnMessageNotPublishedBecauseZeroListeners = msg => { warningWasCalled = true; };
            var eventAggregator = new EventAggregator(config);

            eventAggregator.SendMessage<SomeMessage>();

            warningWasCalled.ShouldBeTrue();
        }


        [Fact]
        public void When_object_has_multiple_listeners_should_subscribe_to_all()
        {
            var eventAggregator = new EventAggregator();
            var handler = new SomeMessageHandler2();
            eventAggregator.AddListener(handler);
            eventAggregator.SendMessage<SomeMessage>();
            eventAggregator.SendMessage<SomeMessage2>();

            handler.EventsTrapped.Count().ShouldEqual(2);
        }


        [Fact]
        public void When_object_has_multiple_listeners_defined_in_an_interface_should_subscribe_to_all()
        {
            var eventAggregator = new EventAggregator();
            var handler = new SomeMessageHandler3();
            eventAggregator.AddListener(handler);
            eventAggregator.SendMessage<SomeMessage>();
            eventAggregator.SendMessage<SomeMessage2>();

            handler.EventsTrapped.Count().ShouldEqual(2);
        }

        [Fact]
        public void Should_throw_when_null_listener_added()
        {
            var eventAggregator = new EventAggregator();
            typeof(ArgumentNullException).ShouldBeThrownBy(() => eventAggregator.AddListener(null, null));
        }

        [Fact]
        public void Should_throw_when_listener_with_no_interfaces_added()
        {
            var eventAggregator = new EventAggregator();
            NoListenerInterfaces noListenerInterfaces = new NoListenerInterfaces();
            typeof(ArgumentException).ShouldBeThrownBy(() => eventAggregator.AddListener(noListenerInterfaces, null));
        }

#if ASYNC
        [Fact]
        public void Should_send_message_when_mixing_async_nonasync_listeners()
        {
            var someMessageHandler = new SomeMessageHandler();
            var someMessageHandlerAsync = new SomeMessageHandlerAsync();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(someMessageHandlerAsync);
            eventAggregator.SendMessage<SomeMessage>();
            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);
            someMessageHandlerAsync.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public async Task Should_send_message_async()
        {
            var someMessageHandler = new SomeMessageHandlerAsync();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await eventAggregator.SendMessageAsync<SomeMessage>();
            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public void Should_not_cause_deadlocks_when_send_async_to_mixed_listeners()
        {
            var someMessageHandler = new SomeMessageHandler6Async();
            var otherMessageHandler = new SomeMessageHandler2();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(otherMessageHandler);

            eventAggregator.SendMessageAsync(new SomeMessage3(eventAggregator)).Wait();
            otherMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public void Should_not_cause_deadlocks_when_send_sync_to_mixed_listeners()
        {
            var someMessageHandler = new SomeMessageHandler6Async();
            var otherMessageHandler = new SomeMessageHandler2();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(otherMessageHandler);

            eventAggregator.SendMessage(new SomeMessage3(eventAggregator));
            otherMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public void Should_not_cause_deadlocks_when_send_async_to_async_listeners()
        {
            var someMessageHandler = new SomeMessageHandler6Async();
            var otherMessageHandler = new SomeMessageHandler2Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(otherMessageHandler);

            eventAggregator.SendMessageAsync(new SomeMessage3(eventAggregator)).Wait();
            otherMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public void Should_not_cause_deadlocks_when_send_sync_to_async_listeners()
        {
            var someMessageHandler = new SomeMessageHandler6Async();
            var otherMessageHandler = new SomeMessageHandler2Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(otherMessageHandler);

            eventAggregator.SendMessage(new SomeMessage3(eventAggregator));
            otherMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

#if UNWRAP_EX
        [Fact]
        public async Task Should_throws_invalid_operation_exception_when_send_async_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler4();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<InvalidOperationException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public async Task Should_throws_invalid_operation_exception_when_send_async_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler4Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<InvalidOperationException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public void Should_throws_invalid_operation_exception_when_send_sync_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler4Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<InvalidOperationException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

        [Fact]
        public async Task Should_throws_real_exception_when_send_async_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler5();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<RealException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public async Task Should_throws_real_exception_when_send_async_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler5Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<RealException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public void Should_throws_real_exception_when_send_sync_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler5Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<RealException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

        [Fact]
        public async Task Should_not_cause_deadlocks_when_send_async_to_sync_listeners()
        {
            var someMessageHandler = new SomeMessageHandler6();
            var otherMessageHandler = new SomeMessageHandler2();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(otherMessageHandler);

            await eventAggregator.SendMessageAsync(new SomeMessage3(eventAggregator));
            otherMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

#else
        [Fact]
        public async Task Should_throws_target_invocation_exception_when_send_async_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler4();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<TargetInvocationException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public async Task Should_throws_invalid_operation_exception_when_send_async_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler4Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<InvalidOperationException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public void Should_throws_invalid_operation_exception_when_send_sync_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler4Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<InvalidOperationException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

        [Fact]
        public async Task Should_throws_real_exception_when_send_async_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler5Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            await Assert.ThrowsAsync<RealException>(() => eventAggregator.SendMessageAsync<SomeMessage>());
        }

        [Fact]
        public async Task Should_throws_target_invocation_exception_with_real_inner_exception_when_send_async_to_faulty_sync_listener()
        {
            var someMessageHandler = new SomeMessageHandler5();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);

            try
            {
                await eventAggregator.SendMessageAsync<SomeMessage>();
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsType<RealException>(ex.InnerException);
            }
        }

        [Fact]
        public void Should_throws_real_exception_when_send_sync_to_faulty_async_listener()
        {
            var someMessageHandler = new SomeMessageHandler5Async();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            Assert.Throws<RealException>(() => eventAggregator.SendMessage<SomeMessage>());
        }

#endif

        [Fact]
        public async Task Should_send_message_async_when_mixing_async_nonasync_listeners()
        {
            var someMessageHandler = new SomeMessageHandler();
            var someMessageHandlerAsync = new SomeMessageHandlerAsync();
            var eventAggregator = new EventAggregator();

            eventAggregator.AddListener(someMessageHandler);
            eventAggregator.AddListener(someMessageHandlerAsync);
            await eventAggregator.SendMessageAsync<SomeMessage>();
            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);
            someMessageHandlerAsync.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public async Task When_a_listener_has_been_garbage_collected_and_an_event_is_published_async_the_zombied_handler_should_be_removed()
        {
            var eventAggregator = new EventAggregator();

            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator);
            GC.Collect();

            await eventAggregator.SendMessageAsync<SomeMessage>();

            eventAggregator.GetListeners().Count().ShouldEqual(0);
        }

        [Fact]
        public async Task When_instructed_to_hold_a_strong_reference_by_default_and_the_listener_is_attempted__been_garbage_collected_and_an_event_is_published_async_the_zombied_handler_should_be_removed()
        {
            var config = new EventAggregator.Config
            {
                HoldReferences = true
            };
            var eventAggregator = new EventAggregator(config);

            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator, null);
            GC.Collect();

            await eventAggregator.SendMessageAsync<SomeMessage>();

            eventAggregator.GetListeners().Count().ShouldEqual(1);
        }

        [Fact]
        public async Task When_instructed_to_hold_a_strong_reference_and_the_listener_is_attempted__been_garbage_collected_and_an_event_is_published_async_the_zombied_handler_should_be_removed()
        {
            var config = new EventAggregator.Config
            {
                HoldReferences = true
            };
            var eventAggregator = new EventAggregator(config);

            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator, true);
            GC.Collect();

            await eventAggregator.SendMessageAsync<SomeMessage>();

            eventAggregator.GetListeners().Count().ShouldEqual(1);
        }

        [Fact]
        public async Task Can_remove_a_good_listener_with_a_zombied_listener_async()
        {
            var eventAggregator = new EventAggregator();

            SomeMessageHandler2Async messageHandler2 = new SomeMessageHandler2Async();
            AddHandlerInScopeThatWillRemoveInstanceWhenGarbageCollected(eventAggregator, false);
            eventAggregator.AddListener(messageHandler2);
            GC.Collect();

            // both good and zombied listeners
            eventAggregator.GetListeners().Count().ShouldEqual(2);

            // should not throw if removing a good listener
            eventAggregator.RemoveListener(messageHandler2);

            // should be only zombie left
            eventAggregator.GetListeners().Count().ShouldEqual(1);
            await eventAggregator.SendMessageAsync<SomeMessage>();

            // after call to SendMessage, the zombied listener should be removed.
            eventAggregator.GetListeners().Count().ShouldEqual(0);
        }

        [Fact]
        public async Task Can_unsubscribe_manually_async()
        {
            var someMessageHandler = new SomeMessageHandlerAsync();
            var eventAggregator = new EventAggregator();
            eventAggregator.AddListener(someMessageHandler);
            await eventAggregator.SendMessageAsync<SomeMessage>();
            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);


            eventAggregator.RemoveListener(someMessageHandler);
            await eventAggregator.SendMessageAsync<SomeMessage>();

            someMessageHandler.EventsTrapped.Count().ShouldEqual(1);
        }

        [Fact]
        public async Task When_no_subscribers_can_detect_nothing_was_published_async()
        {
            var config = new EventAggregator.Config();
            bool warningWasCalled = false;
            config.OnMessageNotPublishedBecauseZeroListeners = msg => { warningWasCalled = true; };
            var eventAggregator = new EventAggregator(config);

            await eventAggregator.SendMessageAsync<SomeMessage>();

            warningWasCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task When_object_has_multiple_listeners_should_subscribe_to_all_async()
        {
            var eventAggregator = new EventAggregator();
            var handler = new SomeMessageHandler2Async();
            eventAggregator.AddListener(handler);
            await eventAggregator.SendMessageAsync<SomeMessage>();
            await eventAggregator.SendMessageAsync<SomeMessage2>();

            handler.EventsTrapped.Count().ShouldEqual(2);
        }

        [Fact]
        public async Task When_object_has_multiple_listeners_defined_in_an_interface_should_subscribe_to_all_async()
        {
            var eventAggregator = new EventAggregator();
            var handler = new SomeMessageHandler3Async();
            eventAggregator.AddListener(handler);
            await eventAggregator.SendMessageAsync<SomeMessage>();
            await eventAggregator.SendMessageAsync<SomeMessage2>();

            handler.EventsTrapped.Count().ShouldEqual(2);
        }

#endif

        public class NoListenerInterfaces // No IListener interfaces
        {
        }
    }

    public static class EventAggregatorTestExtensions
    {
        public static IEnumerable<object> GetListeners(this IEventSubscriptionManager eventSubscriptionManager)
        {
            var field = eventSubscriptionManager.GetType()
                .GetField("_listeners", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return (IEnumerable<object>) field.GetValue(eventSubscriptionManager);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventAggregatorNet.Tests
{
    public class SomeMessageHandler : IListener<SomeMessage>
    {
        private readonly List<SomeMessage> _eventsTrapped = new List<SomeMessage>();

        public IEnumerable<SomeMessage> EventsTrapped { get { return _eventsTrapped; } }

        public void Handle(SomeMessage message)
        {
            _eventsTrapped.Add(message);
        }
    }

    public class SomeMessageHandler2 :
        IListener<SomeMessage>,
        IListener<SomeMessage2>
    {
        private readonly List<object> _eventsTrapped = new List<object>();

        public IEnumerable<object> EventsTrapped { get { return _eventsTrapped; } }

        public void Handle(SomeMessage message)
        {
            _eventsTrapped.Add(message);
        }

        public void Handle(SomeMessage2 message)
        {
            _eventsTrapped.Add(message);
        }
    }

    public interface IHandlerOfMultipleMessages : IListener<SomeMessage>,
        IListener<SomeMessage2>
    { }

    public class SomeMessageHandler3 : IHandlerOfMultipleMessages
    {
        private readonly List<object> _eventsTrapped = new List<object>();

        public IEnumerable<object> EventsTrapped { get { return _eventsTrapped; } }

        public void Handle(SomeMessage message)
        {
            _eventsTrapped.Add(message);
        }

        public void Handle(SomeMessage2 message)
        {
            _eventsTrapped.Add(message);
        }
    }

    public class SomeMessageHandler4 : IListener<SomeMessage>
    {
        public void Handle(SomeMessage message)
        {
            throw new InvalidOperationException("");
        }
    }

    public class RealException : Exception { }

    public class SomeMessageHandler5 : IListener<SomeMessage>
    {
        public void Handle(SomeMessage message)
        {
            throw new RealException();
        }
    }

    public class SomeMessageHandler6 : IListener<SomeMessage3>
    {
        public void Handle(SomeMessage3 message)
        {
            message.EventAggregator.SendMessage<SomeMessage2>();
        }
    }

#if ASYNC
    public class SomeMessageHandlerAsync : IListenerAsync<SomeMessage>
    {
        private readonly List<SomeMessage> _eventsTrapped = new List<SomeMessage>();

        public IEnumerable<SomeMessage> EventsTrapped { get { return _eventsTrapped; } }

        public async Task Handle(SomeMessage message)
        {
            await Task.Delay(500);
            _eventsTrapped.Add(message);
        }
    }

    public class SomeMessageHandler2Async :
        IListenerAsync<SomeMessage>,
        IListenerAsync<SomeMessage2>
    {
        private readonly List<object> _eventsTrapped = new List<object>();

        public IEnumerable<object> EventsTrapped { get { return _eventsTrapped; } }

        public async Task Handle(SomeMessage message)
        {
            await Task.Delay(500);
            _eventsTrapped.Add(message);
        }

        public async Task Handle(SomeMessage2 message)
        {
            await Task.Delay(500);
            _eventsTrapped.Add(message);
        }
    }

    public interface IHandlerOfMultipleMessagesAsync : IListenerAsync<SomeMessage>,
       IListenerAsync<SomeMessage2>
    { }

    public class SomeMessageHandler3Async : IHandlerOfMultipleMessagesAsync
    {
        private readonly List<object> _eventsTrapped = new List<object>();

        public IEnumerable<object> EventsTrapped { get { return _eventsTrapped; } }

        public async Task Handle(SomeMessage message)
        {
            await Task.Delay(500);
            _eventsTrapped.Add(message);
        }

        public async Task Handle(SomeMessage2 message)
        {
            await Task.Delay(500);
            _eventsTrapped.Add(message);
        }
    }

    public class SomeMessageHandler4Async : IListenerAsync<SomeMessage>
    {
        public async Task Handle(SomeMessage message)
        {
            await Task.Delay(500);
            throw new InvalidOperationException("");
        }
    }

    public class SomeMessageHandler5Async : IListenerAsync<SomeMessage>
    {
        public async Task Handle(SomeMessage message)
        {
            await Task.Delay(500);
            throw new RealException();
        }
    }

    public class SomeMessageHandler6Async : IListenerAsync<SomeMessage3>
    {
        public async Task Handle(SomeMessage3 message)
        {
            await Task.Delay(500);
            await message.EventAggregator.SendMessageAsync<SomeMessage2>();
        }
    }
#endif

}
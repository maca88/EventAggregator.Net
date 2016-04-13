using System;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
namespace EventAggregatorNet
{
    public static class EventAggregatorExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IDisposable AddListenerAction<T>(this IEventSubscriptionManager eventAggregator, Action<T> listener)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (listener == null) throw new ArgumentNullException("listener");

            var delegateListener = new DelegateListener<T>(listener, eventAggregator);
            eventAggregator.AddListener(delegateListener);

            return delegateListener;
        }
#if ASYNC
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IDisposable AddListenerAction<T>(this IEventSubscriptionManager eventAggregator, Func<T, Task> listener)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (listener == null) throw new ArgumentNullException("listener");

            var delegateListener = new DelegateListenerAsync<T>(listener, eventAggregator);
            eventAggregator.AddListener(delegateListener);

            return delegateListener;
        }
#endif
    }

#if ASYNC
    public class DelegateListenerAsync<T> : IListenerAsync<T>, IDisposable
    {
        private readonly Func<T, Task> _listener;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;

        public DelegateListenerAsync(Func<T, Task> listener, IEventSubscriptionManager eventSubscriptionManager)
        {
            _listener = listener;
            _eventSubscriptionManager = eventSubscriptionManager;
        }

        public async Task Handle(T message)
        {
            await _listener(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventSubscriptionManager.RemoveListener(this);
            }
        }
    }
#endif

    public class DelegateListener<T> : IListener<T>, IDisposable
    {
        private readonly Action<T> _listener;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;

        public DelegateListener(Action<T> listener, IEventSubscriptionManager eventSubscriptionManager)
        {
            _listener = listener;
            _eventSubscriptionManager = eventSubscriptionManager;
        }

        public void Handle(T message)
        {
            _listener(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventSubscriptionManager.RemoveListener(this);
            }
        }
    }
}
// ReSharper enable InconsistentNaming

namespace EventAggregatorNet.Tests
{
    public class SomeMessage { }
    public class SomeMessage2 { }

    public class SomeMessage3
    {
        public SomeMessage3(EventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }

        public EventAggregator EventAggregator { get; }
    }
}
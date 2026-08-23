using System.Diagnostics.Metrics;

namespace Census.Shared.Observability
{
    public sealed class EventBusMetrics
    {
        public const string MeterName = "Census.EventBus";

        private static readonly Meter Meter = new(MeterName);
        private readonly Counter<long> _messagesPublished;
        private readonly Counter<long> _messagesConsumed;
        private readonly Counter<long> _messagesFailed;
        private readonly Counter<long> _messagesRetried;
        private readonly Counter<long> _messagesDeadLettered;
        private readonly Counter<long> _messagesDuplicate;

        public EventBusMetrics()
        {
            _messagesPublished = Meter.CreateCounter<long>("census.messages.published");
            _messagesConsumed = Meter.CreateCounter<long>("census.messages.consumed");
            _messagesFailed = Meter.CreateCounter<long>("census.messages.failed");
            _messagesRetried = Meter.CreateCounter<long>("census.messages.retried");
            _messagesDeadLettered = Meter.CreateCounter<long>("census.messages.dead_lettered");
            _messagesDuplicate = Meter.CreateCounter<long>("census.messages.duplicate");
        }

        public void RecordPublished(string eventType) =>
            _messagesPublished.Add(1, new KeyValuePair<string, object?>("event.type", eventType));

        public void RecordConsumed(string eventType) =>
            _messagesConsumed.Add(1, new KeyValuePair<string, object?>("event.type", eventType));

        public void RecordFailed(string eventType) =>
            _messagesFailed.Add(1, new KeyValuePair<string, object?>("event.type", eventType));

        public void RecordRetried(string eventType) =>
            _messagesRetried.Add(1, new KeyValuePair<string, object?>("event.type", eventType));

        public void RecordDeadLettered(string eventType) =>
            _messagesDeadLettered.Add(1, new KeyValuePair<string, object?>("event.type", eventType));

        public void RecordDuplicate(string eventType) =>
            _messagesDuplicate.Add(1, new KeyValuePair<string, object?>("event.type", eventType));
    }
}

using System;

namespace AcklenAvenue.EventSourcing
{
    public class QueueItem<TId>
    {
        public QueueItem(TId aggregateId, object @event, DateTime time)
        {
            AggregateId = aggregateId;
            Event = @event;
            Time = time;
        }

        public TId AggregateId { get; set; }
        public object Event { get; set; }
        public DateTime Time { get; set; }
    }
}
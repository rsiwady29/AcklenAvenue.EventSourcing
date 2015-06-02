using System;

namespace AcklenAvenue.EventSourcing
{
    public class QueueItem
    {
        public QueueItem(Guid aggregateId, object @event, DateTime time)
        {
            AggregateId = aggregateId;
            Event = @event;
            Time = time;
        }

        public Guid AggregateId { get; set; }
        public object Event { get; set; }
        public DateTime Time { get; set; }
    }
}
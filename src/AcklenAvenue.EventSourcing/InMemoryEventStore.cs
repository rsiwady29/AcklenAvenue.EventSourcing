using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcklenAvenue.EventSourcing
{
    public class InMemoryEventStore : IEventStore
    {
        public static readonly List<QueueItem> Items = new List<QueueItem>();

        public Task<IEnumerable<object>> GetStream(Guid aggregateId)
        {
            var task = new Task<IEnumerable<object>>(
                () =>
                {
                    var queueItems = Items.Where(x => x.AggregateId == aggregateId);
                    var domainEvents = queueItems.OrderBy(x => x.Time).Select(x => x.Event);
                    return domainEvents;
                });

            task.Start();

            return task;
        }

        public void Persist(Guid aggregateId, object @event)
        {
            Items.Add(new QueueItem(aggregateId, @event, DateTime.Now));
        }
    }
}
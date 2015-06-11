using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcklenAvenue.EventSourcing
{
    public class InMemoryEventStore<TId> : IEventStore<TId>
    {
        public static readonly List<QueueItem<TId>> Items = new List<QueueItem<TId>>();

        public Task<IEnumerable<object>> GetStream(TId aggregateId)
        {
            var task = new Task<IEnumerable<object>>(
                () =>
                    {
                        IEnumerable<QueueItem<TId>> queueItems = Items.Where(x => Equals(x.AggregateId, aggregateId));
                        IEnumerable<object> domainEvents = queueItems.OrderBy(x => x.Time).Select(x => x.Event);
                        return domainEvents;
                    });

            task.Start();

            return task;
        }

        public void Persist(TId aggregateId, object @event)
        {
            Items.Add(new QueueItem<TId>(aggregateId, @event, DateTime.Now));
        }
    }
}
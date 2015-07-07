using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcklenAvenue.EventSourcing
{
    public interface IEventStore<TId>
    {
        Task<IEnumerable<object>> GetStream(TId aggregateId);
        Task Persist(TId aggregateId, object @event);
        Task PersistInBatch(IEnumerable<InBatchEvent<TId>> batchEvents);
    }
}
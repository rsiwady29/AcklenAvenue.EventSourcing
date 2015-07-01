using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcklenAvenue.EventSourcing
{
    public interface IEventStore<TId>
    {
        Task<IEnumerable<object>> GetStream(TId aggregateId);

        void Persist(TId aggregateId, object @event);

        void PersistInBach(IEnumerable<InBatchEvent<TId>> batchEvents);
    }
}
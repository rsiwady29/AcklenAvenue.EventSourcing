using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcklenAvenue.EventSourcing
{
    public interface IEventStore<in TId>
    {
        Task<IEnumerable<object>> GetStream(TId aggregateId);
        void Persist(TId aggregateId, object @event);
    }
}
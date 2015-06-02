using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcklenAvenue.EventSourcing
{
    public interface IEventStore
    {
        Task<IEnumerable<object>> GetStream(Guid aggregateId);
        void Persist(Guid aggregateId, object @event);
    }
}
namespace AcklenAvenue.EventSourcing
{
    public class InBatchEvent<TId>
    {
        public TId AggregateId { get; set; }

        public object Event { get; set; }
    }
}
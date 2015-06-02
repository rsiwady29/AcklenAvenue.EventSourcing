using System;

namespace AcklenAvenue.EventSourcing.MySql.Specs.Integration
{
    public class TestAggregateCreated
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public TestAggregateCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
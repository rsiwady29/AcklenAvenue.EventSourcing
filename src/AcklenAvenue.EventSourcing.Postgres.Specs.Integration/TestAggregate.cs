using System;
using System.Collections.Generic;

namespace AcklenAvenue.EventSourcing.Postgres.Specs.Integration
{
    public class TestAggregate : AggregateRoot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Gender Gender { get; private set; }

        public TestAggregate(Guid id, string name, Gender gender):base(new List<object>())
        {
            Gender = gender;
            When(NewEvent(new TestAggregateCreated(id, name)));
        }

        void When(TestAggregateCreated newEvent)
        {
            Id = newEvent.Id;
            Name = newEvent.Name;
        }
    }
}
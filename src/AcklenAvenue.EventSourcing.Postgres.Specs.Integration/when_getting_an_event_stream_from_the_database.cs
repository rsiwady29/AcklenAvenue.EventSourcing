using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;

namespace AcklenAvenue.EventSourcing.Postgres.Specs.Integration
{
    public class when_getting_an_event_stream_from_the_database
    {
        static IEventStore<Guid> _eventStore;

        static Guid _id;

        static Task<IEnumerable<object>> _result;

        Establish context =
            () =>
            {
                _eventStore =
                    new TestPostgresEventStore();

                _id = Guid.NewGuid();
                var aggregate = new TestAggregate(_id, "test", new Gender("female"));
                _eventStore.Persist(_id, aggregate.Changes.First());
            };

        Because of = () => _result = _eventStore.GetStream(_id);

        It should_return_the_expected_events = () => _result.Result.First().Should().BeOfType<TestAggregateCreated>();
    }
}
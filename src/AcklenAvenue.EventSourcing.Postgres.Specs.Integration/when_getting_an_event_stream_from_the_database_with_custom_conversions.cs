using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcklenAvenue.EventSourcing.Serializer.JsonNet;
using FluentAssertions;
using Machine.Specifications;

namespace AcklenAvenue.EventSourcing.Postgres.Specs.Integration
{
    public class when_getting_an_event_stream_from_the_database_with_custom_conversions
    {
        static IEventStore<Guid> _eventStore;

        static Guid _id;

        static Task<IEnumerable<object>> _result;

        static TestAggregate _aggregate;

        Establish context =
            () =>
            {
                _eventStore = new TestPostgresEventStore();

                _id = Guid.NewGuid();
                _aggregate = new TestAggregate(_id, "test", new Gender("male"));
                _eventStore.Persist(_id, _aggregate.Changes.First());

                JsonEventConverter.CustomConversions.Add(typeof (Gender), o => new Gender(o.ToString()));
            };

        Because of = () => _result = _eventStore.GetStream(_id);

        It should_return_item_like_saved_event = () => _result.Result.First().ShouldBeLike(_aggregate.Changes.First());

        It should_return_the_expected_events = () => _result.Result.First().Should().BeOfType<TestAggregateCreated>();
    }
}
using System;
using AcklenAvenue.EventSourcing.Serializer.JsonNet;
using Newtonsoft.Json;

namespace AcklenAvenue.EventSourcing.Postgres.Specs.Integration
{
    public class TestPostgresEventStore : PostgresEventStore<Guid>
    {
        JsonEventConverter _jsonEventConverter;

        public TestPostgresEventStore()
            : base("Server=127.0.0.1;Port=5432;User Id=root;Password=00010011;Database=Identity;", "aggregateEvents")
        {
            _jsonEventConverter = new JsonEventConverter();
        }

        public override object DeserializeEvent(JsonEvent eventJson)
        {
            return _jsonEventConverter.GetEvent(eventJson);
        }
    }
}
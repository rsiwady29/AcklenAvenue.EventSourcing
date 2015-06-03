using System;

namespace AcklenAvenue.EventSourcing
{
    public class JsonEvent
    {
        public JsonEvent(string type, string json, DateTime dateTime)
        {
            Type = type;
            Json = json;
            DateTime = dateTime;
        }

        public string Type { get; set; }
        public string Json { get; private set; }
        public DateTime DateTime { get; private set; }
    }
}
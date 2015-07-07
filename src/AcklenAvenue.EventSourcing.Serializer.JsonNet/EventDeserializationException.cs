using System;

namespace AcklenAvenue.EventSourcing.Serializer.JsonNet
{
    public class EventDeserializationException : Exception
    {
        public EventDeserializationException(string originalObjectValue, string targetType, Exception exception)
            : base(string.Format("Trying to convert '{0}' to {1}. You might need to add a custom json conversion.", originalObjectValue
                , targetType), exception)
        {
        }
    }
}
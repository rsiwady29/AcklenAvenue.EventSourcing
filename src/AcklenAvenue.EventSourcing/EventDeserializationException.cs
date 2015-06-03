using System;

namespace AcklenAvenue.EventSourcing
{
    public class EventDeserializationException : Exception
    {
        public EventDeserializationException(string originalObjectValue, string targetType, Exception exception)
            : base(string.Format("Trying to convert '{0}' to {1}.", originalObjectValue
                , targetType), exception)
        {
        }
    }
}
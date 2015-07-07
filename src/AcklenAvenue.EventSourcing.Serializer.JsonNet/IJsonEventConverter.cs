namespace AcklenAvenue.EventSourcing.Serializer.JsonNet
{
    public interface IJsonEventConverter
    {
        object GetEvent(JsonEvent jsonEvent);
    }
}
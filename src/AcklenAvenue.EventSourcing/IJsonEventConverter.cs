namespace AcklenAvenue.EventSourcing
{
    public interface IJsonEventConverter
    {
        object GetEvent(JsonEvent jsonEvent);
    }
}
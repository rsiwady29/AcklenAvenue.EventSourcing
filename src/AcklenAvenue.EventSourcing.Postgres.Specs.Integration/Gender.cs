namespace AcklenAvenue.EventSourcing.Postgres.Specs.Integration
{
    public class Gender
    {
        public string G { get; private set; }

        public Gender(string gender)
        {
            G = gender;
        }
    }
}
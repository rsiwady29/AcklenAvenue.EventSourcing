using System;

namespace AcklenAvenue.EventSourcing.Specs
{
    public class SquirrelCreated
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public SquirrelCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
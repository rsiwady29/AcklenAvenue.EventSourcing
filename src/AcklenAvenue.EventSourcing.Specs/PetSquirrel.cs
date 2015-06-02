using System;
using System.Collections.Generic;

namespace AcklenAvenue.EventSourcing.Specs
{
    public class PetSquirrel : AggregateRoot
    {
        readonly List<string> _meals = new List<string>();

        public PetSquirrel(IEnumerable<object> events) : base(events)
        {
        }

        public PetSquirrel(Guid id, string name) : base(new List<object>())
        {
            When(NewEvent(new SquirrelCreated(id, name)));
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public List<string> Meals
        {
            get { return _meals; }            
        }

        public string Position { get; private set; }

        void When(SquirrelCreated newEvent)
        {
            Id = newEvent.Id;
            Name = newEvent.Name;
        }

        public void Eat(string food)
        {
            When(NewEvent(new SquirrelAte(food)));
        }

        void When(SquirrelAte newEvent)
        {
            Meals.Add(newEvent.Food);
        }

        public void ClimbTree()
        {
            When(NewEvent(new TreeClimbed()));
        }

        void When(TreeClimbed newEvent)
        {
            Position = "tree";
        }
    }
}
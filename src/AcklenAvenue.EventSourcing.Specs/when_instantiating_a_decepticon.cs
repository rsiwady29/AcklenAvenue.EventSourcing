using System;
using System.Collections.Generic;
using FluentAssertions;
using Machine.Specifications;

namespace AcklenAvenue.EventSourcing.Specs
{
    public class when_instantiating_a_decepticon
    {
        static IEnumerable<object> _events;
        static PetSquirrel _decepticonReborn;
        static Guid _id;
        static PetSquirrel _megatron;
        
        Establish context =
            () =>
            {
                _id = Guid.NewGuid();

                _megatron = new PetSquirrel(_id, "Megatron");

                _megatron.Eat("nuts");
                _megatron.ClimbTree();
                
                _events = _megatron.Changes;
            };

        Because of =
            () => _decepticonReborn = new PetSquirrel(_events);

        It should_return_the_same_decepticon_as_far_as_state =
            () => _decepticonReborn.ShouldBeEquivalentTo(_megatron,
                options => options
                    .IgnoringCyclicReferences()
                    .ExcludingFields());
    }
}
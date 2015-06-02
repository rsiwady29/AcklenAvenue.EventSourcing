# AcklenAvenue.EventSourcing

## Wiring up the event store while bootstrapping:

```
public class WireUpEventStore : IBootstrapperTask
    {
        public void Do(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DecepticonRepository>().As<IRepository<Decepticon>>();

            SeedInMemoryEventStore();

            containerBuilder.RegisterType<InMemoryEventStore>().As<IEventStore>();
            //containerBuilder.RegisterType<MySqlEventStore>().As<IEventStore>();
        }

        static void SeedInMemoryEventStore()
        {
            Guid aggregateId = Guid.NewGuid();

            InMemoryEventStore.Items.Add(
                new QueueItem(aggregateId, new DecepticonCreated(aggregateId, "Megatron"), DateTime.Now));

            InMemoryEventStore.Items.Add(
                new QueueItem(aggregateId, new DecepticonAte("apple pie"), DateTime.Now));
        }
    }
```

## A Sample Aggregate Root

```
public class Decepticon : AggregateRoot
    {
        public Decepticon(IEnumerable<object> events) : base(events)
        {
        }

        public Decepticon(Guid id, string name):base(new List<object>())
        {
            When(NewEvent(new DecepticonCreated(id, name)));
        }

        public string Name { get; private set; }
        public string[] Food { get; private set; }
        public EpicBattle CurrentBattle { get; private set; }
        public string[] Wounds { get; private set; }
        public Guid Id { get; private set; }

        void When(DecepticonCreated @event)
        {
            Id = @event.Id;
            Name = @event.Name;
        }

        public void Kill(Autobot autobot)
        {
            When(NewEvent(new DecepticonKilledAutobot(autobot)));
        }

        void When(DecepticonKilledAutobot @event)
        {
            @event.Autobot.Die();
        }

        public void Eat(string food)
        {
            When(NewEvent(new DecepticonAte(food)));
        }

        void When(DecepticonAte @event)
        {
            List<string> list = (Food ?? new string[0]).ToList();
            list.Add(@event.Food);
            Food = list.ToArray();
        }

        public void EnterBattleWithAutobot(Autobot autobot)
        {
            When(NewEvent(new DecepticonEnteredBattleWithAutobot(autobot)));
        }

        void When(DecepticonEnteredBattleWithAutobot @event)
        {
            CurrentBattle = new EpicBattle(this, @event.Autobot);
        }

        public void Wound(string location)
        {
            When(NewEvent(new DecepticonWasWounded(location)));
        }

        void When(DecepticonWasWounded @event)
        {
            List<string> list = (Wounds ?? new string[0]).ToList();
            list.Add(@event.Location);
            Wounds = list.ToArray();
        }
    }
```

## Use the EventStore in your Repository:

```
public class DecepticonRepository : IRepository<Decepticon>
    {
        readonly IEventStore _eventStore;

        public DecepticonRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Decepticon> Get(Guid aggregateId)
        {
            IEnumerable<object> domainEvents = await _eventStore.GetStream(aggregateId);
            List<object> events = domainEvents.ToList();
            return !events.Any() ? new NullDecepticon() : new Decepticon(events);
        }

        public void SaveChanges(Decepticon decepticon)
        {
            foreach (object change in decepticon.Changes)
            {
                _eventStore.Persist(decepticon.Id, change);
            }
        }
    }

    public class NullDecepticon : Decepticon
    {
        public NullDecepticon() : base(Guid.Empty, "Null")
        {
        }

        public NullDecepticon(IEnumerable<object> events) : base(events)
        {
        }

        public NullDecepticon(Guid id, string name) : base(id, name)
        {
        }
    }
```

## Testing the Repository:

```
public class when_getting_a_decepticon_from_the_repository
    {
        static IRepository<Decepticon> _repo;
        static Task<Decepticon> _result;
        static readonly Guid _id = Guid.NewGuid();

        Establish context =
            () =>
            {
                var eventStore = Mock.Of<IEventStore>();

                Mock.Get(eventStore)
                    .Setup(x => x.GetStream(_id))
                    .ReturnsAsync(new List<object>
                                  {
                                      new DecepticonCreated(_id, "Test")
                                  });

                _repo = new DecepticonRepository(eventStore);
            };

        Because of =
            () => _result = _repo.Get(_id);

        It should_return_the_expected_decepticon =
            () => _result.Result.Id.Should().Be(_id);
    }
```

```
public class when_saving_changes
    {
        static DecepticonRepository _repo;
        static IEventStore _eventStore;
        static readonly Guid Id = Guid.NewGuid();
        static Decepticon _decepticon;

        Establish context =
            () =>
            {
                _eventStore = Mock.Of<IEventStore>();
                _repo = new DecepticonRepository(_eventStore);
                _decepticon = new Decepticon(Id, "Test");
                _decepticon.Eat("honey");
            };

        Because of =
            () => _repo.SaveChanges(_decepticon);

        It should_persist_the_first_change_to_the_event_store =
            () =>
                Mock.Get(_eventStore)
                    .Verify(x => x.Persist(Id, Moq.It.Is<object>(y => y.GetType() == typeof(DecepticonCreated))));

        It should_persist_all_changes_to_the_event_store =
            () =>
                Mock.Get(_eventStore)
                    .Verify(x => x.Persist(Id, Moq.It.Is<object>(y => y.GetType() == typeof(DecepticonAte))));
    }
```   

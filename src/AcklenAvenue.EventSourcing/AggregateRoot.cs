using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AcklenAvenue.EventSourcing
{
    public abstract class AggregateRoot
    {
        public List<object> Changes = new List<object>();

        protected AggregateRoot(IEnumerable<object> events)
        {
            foreach (object domainEvent in events)
            {
                Mutate(domainEvent);
            }
        }

        void Mutate(object domainEvent)
        {
            MethodInfo applyMethod = GetMethodForDomainEvent(domainEvent);

            if (applyMethod == null) return;

            applyMethod.Invoke(this, new[] {domainEvent});
        }

        MethodInfo GetMethodForDomainEvent(object domainEvent)
        {
            IEnumerable<MethodInfo> methodInfos = GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "When");

            MethodInfo applyMethod =
                methodInfos.FirstOrDefault(x =>
                                           {
                                               ParameterInfo[] parameterInfos =
                                                   x.GetParameters();

                                               return parameterInfos.First().ParameterType ==
                                                      domainEvent.GetType();
                                           });
            return applyMethod;
        }

        protected T NewEvent<T>(T @event) where T : class
        {
            Changes.Add(@event);
            return @event;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace AcklenAvenue.EventSourcing
{
    public class JsonEventConverter : IJsonEventConverter
    {
        public static IDictionary<Type, Func<object, object>> CustomConversions =
            new Dictionary<Type, Func<object, object>>();
 
        public object GetEvent(JsonEvent jsonEvent)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = assemblies
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.FullName.EndsWith(jsonEvent.Type));

            if (type == null)
                throw new UnknownEventTypeException(jsonEvent.Type, assemblies);

            var objectThatWasDeserialized =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonEvent.Json);

            ConstructorInfo constructorInfo = type.GetConstructors()[0];
            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
            
            object[] paramValues = parameterInfos
                .Select(x =>
                        {
                            var item =
                                objectThatWasDeserialized.FirstOrDefault(y => y.Key.ToLower() == x.Name.ToLower());

                            if (item.Key == null)
                            {
                                throw new Exception(
                                    string.Format(
                                        "When attempting to deserialize the event '{0}' from the event store, no event properties could be found that match ctor arg '{1}'. The event's constructor argument names must match its property names. Ctor args: {2}, Event properties: {3}",
                                        jsonEvent.Type, x.Name,
                                        string.Join(", ", parameterInfos.Select(p => p.Name)),
                                        string.Join(", ", objectThatWasDeserialized.Select(p => p.Key))));
                            }

                            Func<object, object> converter;
                            if (CustomConversions.TryGetValue(x.ParameterType, out converter))
                            {
                                return converter(item.Value);
                            }
                            else{
                                return ConvertFromDefault(x, item);
                            }                            
                        }).ToArray();

            return constructorInfo.Invoke(paramValues);
        }

        static object ConvertFromDefault(ParameterInfo x, KeyValuePair<string, object> item)
        {
            try
            {
                TypeConverter typeConverter =
                    TypeDescriptor.GetConverter(x.ParameterType);
                object fromString =
                    typeConverter.ConvertFromString(
                        (item.Value ?? "").ToString());
                return fromString;
            }
            catch (Exception ex)
            {
                throw new EventDeserializationException((item.Value??"null").ToString(),
                    x.ParameterType.ToString(), ex);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AcklenAvenue.EventSourcing
{
    public class UnknownEventTypeException : Exception
    {
        public UnknownEventTypeException(string type, IEnumerable<Assembly> assemblies):base(string.Format("The type '{0}' was not found when searching assemblies in the AppDomain. The following assemblies were searched: {1}", type, string.Join(", ", assemblies.Select(x=> x.FullName))))
        {
            
        }
    }
}
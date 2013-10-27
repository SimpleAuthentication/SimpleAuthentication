using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthentication.Core
{
    public static class ReflectionHelpers
    {
        public static IList<Type> FindAllTypesOf<T>()
        {
            var type = typeof (T);

            return AppDomain.CurrentDomain
                            .GetAssemblies()
                            .ToList()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract && !p.IsInterface)
                            .ToList();
        }
    }
}
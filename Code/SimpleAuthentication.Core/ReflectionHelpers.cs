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
                            .Where(x =>
                                x.FullName.StartsWith("SimpleAuthentication", StringComparison.OrdinalIgnoreCase)
                                || x.GetReferencedAssemblies().Any(y => y.Name.StartsWith("SimpleAuthentication", StringComparison.OrdinalIgnoreCase)))
                            .SelectMany(s => s.GetTypes())
                            .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract && !p.IsInterface)
                            .ToList();
        }
    }
}
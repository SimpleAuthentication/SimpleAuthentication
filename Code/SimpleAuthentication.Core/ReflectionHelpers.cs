using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleAuthentication.Core
{
    public static class ReflectionHelpers
    {
        public static IList<Type> FindAllTypesOf<T>()
        {
            try
            {
                var type = typeof(T);

                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .ToList()
                    .SelectMany(s => s.GetLoadableTypes())
                    .Where(p => type.IsAssignableFrom(p) &&
                                p.IsClass &&
                                !p.IsAbstract &&
                                !p.IsInterface)
                    .ToList();
            }
            catch (ReflectionTypeLoadException exception)
            {
                var stringBuilder = new StringBuilder();
                foreach (var loaderException in exception.LoaderExceptions)
                {
                    stringBuilder.AppendLine(loaderException.Message);
                    var fileNotFoundException = loaderException as FileNotFoundException;
                    if (fileNotFoundException != null)
                    {
                        if (!string.IsNullOrEmpty(fileNotFoundException.FusionLog))
                        {
                            stringBuilder.AppendLine("Fusion Log:");
                            stringBuilder.AppendLine(fileNotFoundException.FusionLog);
                        }
                    }
                    stringBuilder.AppendLine();
                }

                throw new Exception(
                    "Failed to reflect on the current domain's Assemblies while searching for plugins. Error Message: " +
                    stringBuilder);
            }
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}

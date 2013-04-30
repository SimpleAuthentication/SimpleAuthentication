using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;

namespace WorldDomination.Web.Authentication
{
    public static class MefHelpers
    {
        public static IList<Type> GetExportedTypes<T>()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var catalog = new AggregateCatalog(new DirectoryCatalog(path, "*"));

            return catalog.Parts
                          .Select(part => ComposablePartExportType<T>(part))
                          .Where(t => t != null)
                          .ToList();
        }

        private static Type ComposablePartExportType<T>(ComposablePartDefinition part)
        {
            return part.ExportDefinitions.Any(
                def => def.Metadata.ContainsKey("ExportTypeIdentity") &&
                       def.Metadata["ExportTypeIdentity"].Equals(typeof (T).FullName))
                       ? ReflectionModelServices.GetPartType(part).Value
                       : null;
        }
    }
}
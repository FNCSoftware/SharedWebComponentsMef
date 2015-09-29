using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using SharedWebComponents.Contracts;

namespace SharedWebComponents.Infrastructure.Mef {
    public class MefBootstrapper {
        static bool isLoaded;
        static CompositionHost container;

        public static void Compose(IEnumerable<Assembly> assemblies) {
            if (isLoaded) {
                return;
            }

            container = GetContainer(assemblies);

            isLoaded = true;
        }

        static CompositionHost GetContainer(IEnumerable<Assembly> assemblies) {
            var conventions = new ConventionBuilder();
            conventions.ForTypesDerivedFrom<IUrlProvider>().Export<IUrlProvider>();
            conventions.ForTypesDerivedFrom<IController>().Export<IController>();
            return new ContainerConfiguration().WithAssemblies(assemblies, conventions).CreateContainer();
        }

        public static T GetInstance<T>(string assemblyHint = null, string typeName = null) {
            var type = default(T);
            if (container == null) {
                return type;
            }

            if (string.IsNullOrWhiteSpace(typeName)) {
                type = container.GetExport<T>();
            } else {
                var exports = container.GetExports<T>();
                type = exports.FirstOrDefault(x => x.GetType().Name == typeName && (assemblyHint == null || (x.GetType().AssemblyQualifiedName != null && x.GetType().AssemblyQualifiedName.Contains(assemblyHint))));
            }

            return type;
        }
    }
}
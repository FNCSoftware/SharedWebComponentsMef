using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using SharedWebComponents.Infrastructure.EmbeddedResource;

namespace SharedWebComponents.Infrastructure.Mef {
    public class MefConfig {
        public static void Register() {
            var assemblies = MefAssemblyFetcher.Fetch().ToList();
            foreach (var assembly in assemblies) {
                //by convention, everything before dot will be client url parameter
                var clientUrlParameter = assembly.GetName().Name.Split('.').First();
                UrlToAssemblyMapping.Add(clientUrlParameter, assembly);
            }
            MefBootstrapper.Compose(assemblies);
            EmbeddedResourceRouteConfig.Register();
            ControllerBuilder.Current.SetControllerFactory(new MefFallbackControllerFactory());
            ViewEngines.Engines.Add(new SharedWebComponentsViewEngine("Plugins"));
            HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourceVirtualPathProvider(new EmbeddedResourceNameResolver(assemblies)));
        }
    }
}
using System.Web;
using System.Web.Routing;
using SharedWebComponents.Infrastructure.Mef;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    public class EmbeddedResourceRouteHandler : IRouteHandler {
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext) {
            var embeddedResourceNameResolver = new EmbeddedResourceNameResolver(MefAssemblyFetcher.Fetch());
            var virtualFileStreamFetcher = new VirtualFileStreamFetcher();
            return new EmbeddedResourceHttpHandler(requestContext.RouteData, embeddedResourceNameResolver, virtualFileStreamFetcher);
        }
    }
}
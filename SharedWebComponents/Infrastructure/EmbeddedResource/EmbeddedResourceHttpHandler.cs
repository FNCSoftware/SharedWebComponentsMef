using System;
using System.Web;
using System.Web.Routing;
using SharedWebComponents.Extensions;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class EmbeddedResourceHttpHandler : IHttpHandler {
        readonly RouteData _routeData;
        readonly EmbeddedResourceNameResolver _embeddedResourceNameResolver;
        readonly VirtualFileStreamFetcher _streamFetcher;

        public EmbeddedResourceHttpHandler(RouteData routeData, EmbeddedResourceNameResolver embeddedResourceNameResolver, VirtualFileStreamFetcher streamFetcher) {
            _routeData = routeData;
            _embeddedResourceNameResolver = embeddedResourceNameResolver;
            _streamFetcher = streamFetcher;
        }

        public bool IsReusable {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context) {
            var routeDataValues = _routeData.Values;
            var client = routeDataValues["client"].ToString();
            var assemblyName = UrlToAssemblyMapping.GetAssemblyByUrl(client).GetName().Name;
            var fileName = routeDataValues["file"].ToString();
            var fileExtension = routeDataValues["extension"].ToString();
            var prefix = _routeData.DataTokens["name"] + ".";
            var resourcePath = string.Format("{0}.{1}{2}.{3}", assemblyName, prefix, fileName, fileExtension);
            var pathInfo = new EmbeddedResourcePathInfo(resourcePath);

            AssemblyNameResolution assemblyNameResolution;
            if (_embeddedResourceNameResolver.TryResolve(pathInfo, out assemblyNameResolution)) {
                var stream = _streamFetcher.Fetch(assemblyNameResolution.Assembly, assemblyNameResolution.Name);
                context.Response.Clear();
                context.Response.ContentType = MimeTypeMap.GetMimeType(fileExtension);
                stream.CopyTo(context.Response.OutputStream);
            } else {
                throw new Exception("Unable to find resource: " + resourcePath);
            }
        }
    }
}
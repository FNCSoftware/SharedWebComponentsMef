using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    public class EmbeddedResourceHttpHandler : IHttpHandler {
        readonly RouteData _routeData;

        public EmbeddedResourceHttpHandler(RouteData routeData) {
            _routeData = routeData;
        }

        public bool IsReusable {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context) {
            var routeDataValues = _routeData.Values;
            var fileName = routeDataValues["file"].ToString();
            var fileExtension = routeDataValues["extension"].ToString();
            var prefix = _routeData.DataTokens["name"] + ".";
            var nameSpace = Assembly.GetExecutingAssembly().GetName().Name;
            var manifestResourceName = string.Format("{0}.{1}{2}.{3}", nameSpace, prefix, fileName, fileExtension);
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName);
            context.Response.Clear();
            context.Response.ContentType = MimeMapping.GetMimeMapping(fileExtension);
            stream.CopyTo(context.Response.OutputStream);
        }
    }
}
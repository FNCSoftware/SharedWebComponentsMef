using System.Web.Routing;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    public static class EmbeddedResourceRouteConfig {
        static readonly string FileWithExtensionPart = "{file}.{extension}";
        static readonly string ClientPart = "{client}";
        static readonly string Content = "Content";
        static readonly string Scripts = "Scripts";
        static readonly string Images = "Images";
        static readonly string Fonts = "Fonts";

        public static void Register() {
            RouteTable.Routes.Insert(0,
                new Route(GetUrl(Content),
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { extension = "css" }),
                    new RouteValueDictionary(new { name = Content }),
                    new EmbeddedResourceRouteHandler()
                ));
            
            RouteTable.Routes.Insert(0,
                new Route(GetUrl(Scripts),
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { extension = "js" }),
                    new RouteValueDictionary(new { name = Scripts }),
                    new EmbeddedResourceRouteHandler()
                ));
            
            RouteTable.Routes.Insert(0,
                new Route(GetUrl(Images),
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { name = Images }),
                    new EmbeddedResourceRouteHandler()
                ));
            
            RouteTable.Routes.Insert(0,
                new Route(GetUrl(Fonts),
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { name = Fonts }),
                    new EmbeddedResourceRouteHandler()
                ));
        }

        static string GetUrl(string fileTypePart) {
            return string.Format("{0}/{1}/{2}", ClientPart, fileTypePart, FileWithExtensionPart);
        }
    }
}
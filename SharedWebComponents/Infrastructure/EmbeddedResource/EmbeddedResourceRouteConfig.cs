using System.Web.Routing;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    public static class EmbeddedResourceRouteConfig {
        static readonly string FileWithExtension = "/{file}.{extension}";
        static readonly string Content = "Content";
        static readonly string Scripts = "Scripts";
        static readonly string Images = "Images";
        static readonly string Fonts = "Fonts";

        public static void Register() {
            RouteTable.Routes.Insert(0,
                new Route(Content + FileWithExtension,
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { name = Content }),
                    new RouteValueDictionary(new { extension = "css" }),
                    new EmbeddedResourceRouteHandler()
                ));
            
            RouteTable.Routes.Insert(1,
                new Route(Scripts + FileWithExtension,
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { name = Scripts }),
                    new RouteValueDictionary(new { extension = "js" }),
                    new EmbeddedResourceRouteHandler()
                ));
            
            RouteTable.Routes.Insert(2,
                new Route(Images + FileWithExtension,
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { name = Images }),
                    new RouteValueDictionary(new { }),
                    new EmbeddedResourceRouteHandler()
                ));
            
            RouteTable.Routes.Insert(3,
                new Route(Fonts + FileWithExtension,
                    new RouteValueDictionary(new { }),
                    new RouteValueDictionary(new { name = Fonts }),
                    new RouteValueDictionary(new { }),
                    new EmbeddedResourceRouteHandler()
                ));
        }
    }
}
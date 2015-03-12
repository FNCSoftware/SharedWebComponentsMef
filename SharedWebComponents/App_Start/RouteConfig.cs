using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SharedWebComponents
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{client}/{controller}/{action}/{id}",
                defaults: new { client = "Client1", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { client = new IsValidClient() }
            );
        }
    }

    public class IsValidClient : IRouteConstraint {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            return true;
        }
    }
}
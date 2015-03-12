using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using SharedWebComponents.Infrastructure.Mef;

namespace SharedWebComponents
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            MefConfig.Register();
        }
    }
}
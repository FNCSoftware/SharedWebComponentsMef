using System.Web.Mvc;
using SharedWebComponents.Contracts;
using SharedWebComponents.Infrastructure.Mef;

namespace SharedWebComponents.Controllers {
    public class HomeController : Controller {
        public ActionResult Index(string client) {
            return RedirectToAction("Show");
        }

        public ActionResult Show(string client) {
            var destinationUrl = GetUrlProvider(client).GetUrl();
            return Redirect(destinationUrl);
        }

        static IUrlProvider GetUrlProvider(string client = null) {
            var urlProviderName = string.IsNullOrWhiteSpace(client) ? null : client + "UrlProvider";
            var result = MefBootstrapper.GetInstance<IUrlProvider>(client, urlProviderName);
            return result;
        }
    }
}
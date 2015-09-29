using System;
using System.Web.Mvc;
using SharedWebComponents.Infrastructure.Utility;

namespace SharedWebComponents.Controllers {
    public class HomeController : Controller {
        public ActionResult Index(string client) {
            return RedirectToAction("Show");
        }

        public ActionResult Show(string client) {
            var urlProvider = UrlProviderFetcher.Fetch(client, "Show");
            if (urlProvider == null) {
                throw new Exception("Required: Destination must be provided for route: Show");
            }
            var destinationUrl = urlProvider.GetUrl();
            return Redirect(destinationUrl);
        }

        public ActionResult Override(string client) {
            var urlProvider = UrlProviderFetcher.Fetch(client, "Override");
            if (urlProvider == null) {
                return View();
            }
            var destinationUrl = urlProvider.GetUrl();
            return Redirect(destinationUrl);
        }
    }
}
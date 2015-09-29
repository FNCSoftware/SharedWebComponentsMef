using System.Web.Mvc;

namespace Client1.Page.Controllers {
    public class TestController : Controller {
        public ActionResult Index() {
            return View();
        }

        public ActionResult Override() {
            return View();
        }
    }
}
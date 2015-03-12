using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace SharedWebComponents.Infrastructure.Mef {
    public class MefFallbackControllerFactory : IControllerFactory {
        public IController CreateController(RequestContext requestContext, string controllerName) {
            var controllerTypeName = controllerName + "Controller";
            var controller = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => typeof (IController).IsAssignableFrom(x) && x.Name == controllerTypeName);
            if (controller != null) {
                return (IController) Activator.CreateInstanceFrom(controller.Assembly.CodeBase, controller.FullName).Unwrap();
            }

            var clientName = requestContext.RouteData.Values["client"].ToString();
            var pluginController = MefBootstrapper.GetInstance<IController>(clientName, controllerTypeName);

            if (pluginController == null) {
                throw new Exception("Controller not found!");
            }

            return pluginController;
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName) {
            return SessionStateBehavior.Default;
        }

        public void ReleaseController(IController controller) {
            var disposableController = controller as IDisposable;

            if (disposableController != null) {
                disposableController.Dispose();
            }
        }
    }
}
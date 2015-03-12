using System.Collections.Generic;
using System.Web.Mvc;
using SharedWebComponents.Infrastructure.EmbeddedResource;

namespace SharedWebComponents.Infrastructure {
    //http://stackoverflow.com/questions/9838766/implementing-a-custom-razorviewengine
    internal class SharedWebComponentsViewEngine : RazorViewEngine {
        string Location { get; set; }

        SharedWebComponentsViewEngine() {
            AreaViewLocationFormats = AppendLocationFormats(_newAreaViewLocations, AreaViewLocationFormats);
            AreaMasterLocationFormats = AppendLocationFormats(_newAreaMasterLocations, AreaMasterLocationFormats);
            AreaPartialViewLocationFormats = AppendLocationFormats(_newAreaPartialViewLocations, AreaPartialViewLocationFormats);
            ViewLocationFormats = AppendLocationFormats(_newViewLocations, ViewLocationFormats);
            MasterLocationFormats = AppendLocationFormats(_newMasterLocations, MasterLocationFormats);
            PartialViewLocationFormats = AppendLocationFormats(_newPartialViewLocations, PartialViewLocationFormats);
        }

        public SharedWebComponentsViewEngine(string location) : this() {
            Location = location;
        }
        
        readonly string[] _newAreaViewLocations = {
                                                      "~/Areas/{2}/%1Views/{1}/{0}.cshtml",
                                                      "~/Areas/{2}/%1Views/{1}/{0}.vbhtml",
                                                      "~/Areas/{2}/%1Views//Shared/{0}.cshtml",
                                                      "~/Areas/{2}/%1Views//Shared/{0}.vbhtml"
                                                  };

        readonly string[] _newAreaMasterLocations = {
                                                        "~/Areas/{2}/%1Views/{1}/{0}.cshtml",
                                                        "~/Areas/{2}/%1Views/{1}/{0}.vbhtml",
                                                        "~/Areas/{2}/%1Views/Shared/{0}.cshtml",
                                                        "~/Areas/{2}/%1Views/Shared/{0}.vbhtml"
                                                    };

        readonly string[] _newAreaPartialViewLocations = {
                                                             "~/Areas/{2}/%1Views/{1}/{0}.cshtml",
                                                             "~/Areas/{2}/%1Views/{1}/{0}.vbhtml",
                                                             "~/Areas/{2}/%1Views/Shared/{0}.cshtml",
                                                             "~/Areas/{2}/%1Views/Shared/{0}.vbhtml"
                                                         };

        readonly string[] _newViewLocations = {
                                                  "~/%1Views/{1}/{0}.cshtml",
                                                  "~/%1Views/{1}/{0}.vbhtml",
                                                  "~/%1Views/Shared/{0}.cshtml",
                                                  "~/%1Views/Shared/{0}.vbhtml"
                                              };

        readonly string[] _newMasterLocations = {
                                                    "~/%1Views/{1}/{0}.cshtml",
                                                    "~/%1Views/{1}/{0}.vbhtml",
                                                    "~/%1Views/Shared/{0}.cshtml",
                                                    "~/%1Views/Shared/{0}.vbhtml"
                                                };

        readonly string[] _newPartialViewLocations = {
                                                         "~/%1Views/{1}/{0}.cshtml",
                                                         "~/%1Views/{1}/{0}.vbhtml",
                                                         "~/%1Views/Shared/{0}.cshtml",
                                                         "~/%1Views/Shared/{0}.vbhtml"
                                                     };

        static string[] AppendLocationFormats(IEnumerable<string> newLocations, IEnumerable<string> defaultLocations) {
            var viewLocations = new List<string>();
            viewLocations.AddRange(newLocations);
            viewLocations.AddRange(defaultLocations);
            return viewLocations.ToArray();
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {
            return base.CreateView(controllerContext, GetViewPath(viewPath, controllerContext), masterPath);
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath) {
            return base.CreatePartialView(controllerContext, GetViewPath(partialPath, controllerContext));
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
            return base.FileExists(controllerContext, GetViewPath(virtualPath, controllerContext));
        }

        string GetViewPath(string path, ControllerContext controllerContext) {
            var clientUrlParameter = controllerContext.RouteData.Values["client"].ToString();
            var assemblyName = UrlToAssemblyMapping.GetAssemblyByUrl(clientUrlParameter).GetName().Name;
            return path.Replace("%1", Location + "/" + assemblyName + "/");
        }
    }
}
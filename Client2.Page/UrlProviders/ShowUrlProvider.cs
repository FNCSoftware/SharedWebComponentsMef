using SharedWebComponents.Contracts;

namespace Client2.Page.UrlProviders {
    //todo: generic way to generate these strings
    public class ShowUrlProvider : IUrlProvider {
        public string GetUrl() {
            return "/Client2/Test/";
        }
    }
}
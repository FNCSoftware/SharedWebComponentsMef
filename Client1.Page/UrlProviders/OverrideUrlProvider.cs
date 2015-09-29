using SharedWebComponents.Contracts;

namespace Client1.Page.UrlProviders {
    public class OverrideUrlProvider : IUrlProvider {
        public string GetUrl() {
            return "/Client1/Test/Override";
        }
    }
}
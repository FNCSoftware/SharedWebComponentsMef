using SharedWebComponents.Contracts;

namespace Client1.Page.Parts {
    public class Client1UrlProvider : IUrlProvider {
        public string GetUrl() {
            return "/Client1/Test/";
        }
    }
}
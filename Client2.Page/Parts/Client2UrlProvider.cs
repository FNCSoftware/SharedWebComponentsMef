using SharedWebComponents.Contracts;

namespace Client2.Page.Parts {
    public class Client2UrlProvider : IUrlProvider {
        public string GetUrl() {
            return "/Client2/Test/";
        }
    }
}
using SharedWebComponents.Contracts;
using SharedWebComponents.Infrastructure.Mef;

namespace SharedWebComponents.Infrastructure.Utility {
    internal class UrlProviderFetcher {
        public static IUrlProvider Fetch(string client = null, string typePrefix = null) {
            var urlProviderName = string.IsNullOrWhiteSpace(client) ? null : typePrefix + "UrlProvider";
            var result = MefBootstrapper.GetInstance<IUrlProvider>(client, urlProviderName);
            return result;
        }
    }
}
using System.Collections.Generic;
using System.Reflection;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal static class UrlToAssemblyMapping {
        //assumes only one dll per client, for now
        static readonly IDictionary<string, Assembly> LookupByUrl = new Dictionary<string, Assembly>();
        static readonly IDictionary<string, string> LookupByAssemblyName = new Dictionary<string, string>();

        public static void Add(string url, Assembly assembly) {
            LookupByUrl.Add(url.ToLowerInvariant(), assembly);
            LookupByAssemblyName.Add(assembly.GetName().Name.ToLowerInvariant(), url);
        }

        public static Assembly GetAssemblyByUrl(string url) {
            Assembly result;
            if (LookupByUrl.TryGetValue(url.ToLowerInvariant(), out result)) {
                return result;
            }
            return null;
        }

        public static string GetUrlByAssemblyName(string assemblyName) {
            string result;
            if (LookupByAssemblyName.TryGetValue(assemblyName.ToLowerInvariant(), out result)) {
                return result;
            }
            return null;
        }
    }
}
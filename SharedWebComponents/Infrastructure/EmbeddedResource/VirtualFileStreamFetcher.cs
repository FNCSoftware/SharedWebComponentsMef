using System;
using System.IO;
using System.Reflection;
using SharedWebComponents.Extensions;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class VirtualFileStreamFetcher {
        public Stream Fetch(Assembly assembly, string resourceName) {
            var result = assembly.GetManifestResourceStream(resourceName);
            if (resourceName.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase)) {
                result = result.PrependViewStream();
            }
            return result;
        }
    }
}
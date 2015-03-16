using System;
using System.Collections;
using System.Web.Caching;
using System.Web.Hosting;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class EmbeddedResourceVirtualPathProvider : VirtualPathProvider {
        readonly IEmbeddedResourceNameResolver _embeddedResourceNameResolver;

        public EmbeddedResourceVirtualPathProvider(IEmbeddedResourceNameResolver embeddedResourceNameResolver) {
            _embeddedResourceNameResolver = embeddedResourceNameResolver;
        }

        bool IsEmbeddedResourcePath(string virtualPath) {
            //if path is serving a file... need to fail resolve and pass to httpHandler
            if (virtualPath.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) || virtualPath.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }
            AssemblyNameResolution result;
            var pathInfo = new EmbeddedResourcePathInfo(virtualPath);
            return _embeddedResourceNameResolver.TryResolve(pathInfo, out result);
        }

        public override bool FileExists(string virtualPath) {
            return IsEmbeddedResourcePath(virtualPath) || base.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath) {
            if (IsEmbeddedResourcePath(virtualPath)) {
                return new EmbeddedResourceVirtualFile(_embeddedResourceNameResolver, virtualPath);
            }
            var result = base.GetFile(virtualPath);
            return result;
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart) {
            return IsEmbeddedResourcePath(virtualPath) ? null : base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }
}
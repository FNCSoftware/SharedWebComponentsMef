using System;
using System.Collections;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class EmbeddedResourceVirtualPathProvider : VirtualPathProvider {
        readonly IEmbeddedResourceNameResolver _embeddedResourceNameResolver;
        readonly string _embeddedResourceIdentifier;

        public EmbeddedResourceVirtualPathProvider(IEmbeddedResourceNameResolver embeddedResourceNameResolver, string embeddedResourceIdentifier) {
            _embeddedResourceNameResolver = embeddedResourceNameResolver;
            _embeddedResourceIdentifier = embeddedResourceIdentifier;
        }

        bool IsEmbeddedResourcePath(string virtualPath) {
            var appRelativePath = VirtualPathUtility.ToAppRelative(virtualPath);
            AssemblyNameResolution result;
            var pathInfo = new EmbeddedResourcePathInfo(virtualPath);
            return appRelativePath.Contains(_embeddedResourceIdentifier) && _embeddedResourceNameResolver.TryResolve(pathInfo, out result);
        }

        public override bool FileExists(string virtualPath) {
            return base.FileExists(virtualPath) || IsEmbeddedResourcePath(virtualPath);
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
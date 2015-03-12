using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class EmbeddedResourceVirtualFile : VirtualFile {
        readonly IEmbeddedResourceNameResolver _embeddedResourceNameResolver;
        readonly VirtualFileStreamFetcher _streamFetcher;
        readonly EmbeddedResourcePathInfo _pathInfo;

        public EmbeddedResourceVirtualFile(IEmbeddedResourceNameResolver embeddedResourceNameResolver, string virtualPath) : base(virtualPath) {
            _embeddedResourceNameResolver = embeddedResourceNameResolver;
            var path = VirtualPathUtility.ToAppRelative(virtualPath);
            _pathInfo = new EmbeddedResourcePathInfo(path);
            _streamFetcher = new VirtualFileStreamFetcher();
        }

        public override Stream Open() {
            AssemblyNameResolution assemblyNameResolution;
            if (_embeddedResourceNameResolver.TryResolve(_pathInfo, out assemblyNameResolution)) {
                var result = _streamFetcher.Fetch(assemblyNameResolution.Assembly, assemblyNameResolution.Name);
                return result;
            }
            throw new Exception("Unable to open virtual file");
        }
    }
}
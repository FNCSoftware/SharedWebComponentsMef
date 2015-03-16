using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal interface IEmbeddedResourceNameResolver {
        bool TryResolve(EmbeddedResourcePathInfo pathInfo, out AssemblyNameResolution result);
        AssemblyNameResolution Resolve(EmbeddedResourcePathInfo pathInfo);
    }

    internal class EmbeddedResourceNameResolver : IEmbeddedResourceNameResolver {
        Dictionary<string, AssemblyNameResolution> _resourceMap;
        List<AssemblyNameResolution> _resources;

        Dictionary<string, AssemblyNameResolution> ResourceMap {
            get { return _resourceMap ?? (_resourceMap = new Dictionary<string, AssemblyNameResolution>()); }
        }

        List<AssemblyNameResolution> Resources {
            get { return _resources ?? (_resources = new List<AssemblyNameResolution>()); }
        }

        public EmbeddedResourceNameResolver(IEnumerable<Assembly> assemblies) {
            PopulateResources(assemblies);
        }

        void PopulateResources(IEnumerable<Assembly> assemblies) {
            foreach (var assembly in assemblies) {
                foreach (var resourceName in assembly.GetManifestResourceNames()) {
                    Resources.Add(new AssemblyNameResolution(assembly, resourceName));
                }
            }
        }

        public bool TryResolve(EmbeddedResourcePathInfo pathInfo, out AssemblyNameResolution result) {
            try {
                 result = Resolve(pathInfo);
            } catch {
                result = null;
                return false;
            }
            return true;
        }

        public AssemblyNameResolution Resolve(EmbeddedResourcePathInfo pathInfo) {
            var normalizedResourceName = GetNormalizedResourceName(pathInfo);
            AssemblyNameResolution result;
            if (ResourceMap.TryGetValue(normalizedResourceName, out result)) {
                return result;
            }
            foreach (var resource in Resources) {
                if (normalizedResourceName.EndsWith(resource.Name, StringComparison.InvariantCultureIgnoreCase)) {
                    ResourceMap.Add(normalizedResourceName, resource);
                    return resource;
                }
            }
            if (TryGetByName(pathInfo.GetFileName().ToLowerInvariant(), out result)) {
                return result;
            }

            throw new Exception(string.Format("Resource ({0}) did not exist in assembly.", pathInfo.Path));
        }

        string GetNormalizedResourceName(EmbeddedResourcePathInfo pathInfo) {
            var result = pathInfo.Path.Replace("~/", "").Replace("/", ".").ToLowerInvariant();
            return result;
        }

        bool TryGetByName(string resourceName, out AssemblyNameResolution result) {
            result = null;
            var matchingNames = Resources.Where(x => x.Name.EndsWith("." + resourceName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (matchingNames.Count() == 1) {
                result = matchingNames.First();
                return true;
            }
            return false;
        }
    }
    
    internal class AssemblyNameResolution {
        public AssemblyNameResolution(Assembly assembly, string name) {
            Assembly = assembly;
            Name = name;
        }

        public Assembly Assembly { get; set; }
        public string Name { get; set; }
    }
}
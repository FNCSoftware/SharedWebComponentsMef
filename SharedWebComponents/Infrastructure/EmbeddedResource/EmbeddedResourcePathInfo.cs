using System.Linq;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class EmbeddedResourcePathInfo {
        public string Path { get; set; }
        readonly string[] _pathTokens;

        public EmbeddedResourcePathInfo(string path) {
            Path = path;
            _pathTokens = path.Replace("~/", "").Split('/');
        }

        public string GetAssemblyName() {
            if (_pathTokens.Length < 2) {
                return null;
            }
            return _pathTokens[1];
        }

        public string GetFileName() {
            return _pathTokens.Last();
        }
    }
}
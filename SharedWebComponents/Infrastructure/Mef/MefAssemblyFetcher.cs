using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharedWebComponents.Infrastructure.Mef {
    internal class MefAssemblyFetcher {
        public static IEnumerable<Assembly> Fetch() {
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RuntimeDirectoryConstants.MEF_PLUGINS);
            var assemblies = new DirectoryInfo(assemblyPath).GetFiles("*.dll").Select(x => Assembly.LoadFile(x.FullName)).ToList();
            return assemblies;
        } 
    }
}
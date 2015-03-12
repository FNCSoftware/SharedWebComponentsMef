using System.IO;
using SharedWebComponents.Infrastructure.EmbeddedResource;

namespace SharedWebComponents.Extensions {
    public static class StreamExtensions {
        public static Stream PrependViewStream(this Stream source) {
            var view = VirtualFileRazorViewHelper.GetViewString(source);
            var result = new MemoryStream();
            var writer = new StreamWriter(result);
            writer.Write(view);
            writer.Flush();
            result.Position = 0;
            return result;
        }
    }
}
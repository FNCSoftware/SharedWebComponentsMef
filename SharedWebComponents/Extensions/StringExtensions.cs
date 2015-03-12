using System.Text.RegularExpressions;

namespace SharedWebComponents.Extensions {
    public static class StringExtensions {
        public static string BetweenExclusive(this string source, string left, string right) {
            return Between(source, left, right);
        }

        public static string BetweenInclusive(this string source, string left, string right) {
            var match = Between(source, left, right);
            return left + match + right;
        }

        static string Between(string source, string left, string right) {
            return Regex.Match(source, string.Format("{0}(.*){1}", left, right)).Groups[1].Value;
        }
    }
}
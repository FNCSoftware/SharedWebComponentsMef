using System;
using System.IO;
using SharedWebComponents.Extensions;

namespace SharedWebComponents.Infrastructure.EmbeddedResource {
    internal class VirtualFileRazorViewHelper {
        //todo: need to build this from web config dynamically
        static readonly string WebviewpagePrependTemplate = @"@inherits System.Web.Mvc.WebViewPage{0}{1}@using System.Web.Mvc{2}@using System.Web.WebPages{3}@using System.Web.Mvc.Html{4}@using System.Web.Optimization{5}";

        public static string GetViewString(Stream stream) {
            string result;
            using (var reader = new StreamReader(stream)) {
                result = reader.ReadToEnd();
            }
            var modelString = GetModelString(result);
            if (!string.IsNullOrWhiteSpace(modelString)) {
                result = RemoveModelDeclaration(result);
            }
            var inheritsAndModelString = GetInheritsAndModelString(modelString);
            result = inheritsAndModelString + result;
            return result;
        }

        static string GetInheritsAndModelString(string modelString) {
            var result = string.Format(WebviewpagePrependTemplate, modelString, Environment.NewLine, Environment.NewLine, Environment.NewLine, Environment.NewLine, Environment.NewLine);
            return result;
        }

        static string RemoveModelDeclaration(string result) {
            var modelDeclaration = result.BetweenInclusive("@model", Environment.NewLine);
            if (!String.IsNullOrWhiteSpace(modelDeclaration)) {
                result = result.Replace(modelDeclaration, "");
            }
            return result;
        }

        static string GetModelString(string input) {
            var model = input.BetweenExclusive("@model", Environment.NewLine).Trim();
            var type = Type.GetType(model, false);
            if (type == null) {
                return null;
            }
            var result = String.IsNullOrWhiteSpace(model) ? "" : "<" + model + ">";
            return result;
        }
    }
}
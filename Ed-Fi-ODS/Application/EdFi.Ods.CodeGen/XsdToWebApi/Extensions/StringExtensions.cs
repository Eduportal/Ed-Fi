namespace EdFi.Ods.CodeGen.XsdToWebApi.Extensions
{
    using System;
    using System.IO;

    using EdFi.Common.Extensions;

    public static class StringExtensions
    {
        public const string ProjectExtension = "EXTENSION-";
        private static readonly int _projectExtensionLength = ProjectExtension.Length;
        private const string _extensionSuffix = "extension";
        private static readonly int _extensionSuffixLength = _extensionSuffix.Length;
        private const string _restrictionSuffix = "restriction";
        private static readonly int _restrictionSuffixLength = _restrictionSuffix.Length;

        public static string StripProjectPrefix(this string text)
        {
            return text.StartsWith(ProjectExtension) ? text.Substring(_projectExtensionLength, text.Length - _projectExtensionLength) : text;
        }

        public static bool IsExtensionSchema(this string xsdFileName)
        {
            return Path.GetFileName(xsdFileName).StartsWith(ProjectExtension);
        }
        public static string StripExtensionNameValues(this string text)
        {
            var withoutExtensionSuffix = text.EndsWith(_extensionSuffix, StringComparison.InvariantCultureIgnoreCase)
                ? text.Remove(text.Length - _extensionSuffixLength)
                : text;
            var withoutRestrictionSuffix = withoutExtensionSuffix.EndsWith(_restrictionSuffix, StringComparison.InvariantCultureIgnoreCase)
                ? withoutExtensionSuffix.Remove(withoutExtensionSuffix.Length - _restrictionSuffixLength)
                : withoutExtensionSuffix;
            return withoutRestrictionSuffix.StripProjectPrefix();
        }
    }
}

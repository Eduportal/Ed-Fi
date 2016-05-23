using System.IO;
using System.Reflection;

namespace EdFi.Ods.Common.Utils.Extensions
{
    public static class AssemblyExtensions
    {
        public static string ReadResource(this Assembly assembly, string fullyQualifiedResourceName)
        {
            string result;

            using (var stream = assembly.GetManifestResourceStream(fullyQualifiedResourceName))
            using (var reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}
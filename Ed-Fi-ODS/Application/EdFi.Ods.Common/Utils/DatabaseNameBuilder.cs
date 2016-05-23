namespace EdFi.Ods.Common.Utils
{
    public class DatabaseNameBuilder
    {
        public const string EdFiDatabase = "EdFi_Ods";
        
        private const string TemplatePrefix = EdFiDatabase + "_";
        private const string SandboxPrefix = EdFiDatabase + "_Sandbox_";

        public const string TemplateEmptyDatabase = TemplatePrefix + "Empty_Template";
        public const string TemplateMinimalDatabase = TemplatePrefix + "Minimal_Template";
        public const string TemplateSampleDatabase = TemplatePrefix + "Populated_Template";

        public static string SandboxNameForKey(string key)
        {
            return SandboxPrefix + key;
        }

        public static string YearSpecificOdsDatabaseName(int year)
        {
            return TemplatePrefix + year;
        }

        public static string KeyFromSandboxName(string sandboxName)
        {
            return sandboxName.Replace(SandboxPrefix, string.Empty);
        }
    }
}
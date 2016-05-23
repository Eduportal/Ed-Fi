// *************************************************************************
// ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
// *************************************************************************

using System.Configuration;

namespace EdFi.Common.Configuration
{
    public class AppConfigSectionProvider : IConfigSectionProvider
    {
        public object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }
    }
}

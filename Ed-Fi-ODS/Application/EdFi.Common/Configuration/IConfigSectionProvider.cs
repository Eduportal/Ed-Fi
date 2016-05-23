// *************************************************************************
// ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
// *************************************************************************
namespace EdFi.Common.Configuration
{
    public interface IConfigSectionProvider
    {
        object GetSection(string sectionName);
    }
}

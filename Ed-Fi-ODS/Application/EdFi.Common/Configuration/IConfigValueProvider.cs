// *************************************************************************
// ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
// *************************************************************************
namespace EdFi.Common.Configuration
{
    public interface IConfigValueProvider
    {
        string GetValue(string name);
    }
}

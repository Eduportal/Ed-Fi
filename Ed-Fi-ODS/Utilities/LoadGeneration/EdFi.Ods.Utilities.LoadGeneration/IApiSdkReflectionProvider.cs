using System;
using System.Reflection;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IApiSdkReflectionProvider
    {
        ConstructorInfo GetApiConstructorForModelType(Type modelType);
        MethodInfo LocateGetAllMethodFrom(object api, Type modelType);
        MethodInfo LocatePostMethodFrom(object api);
        bool TryGetModelType(string modelName, out Type modelType);
        bool TryGetModelType(Type resourceReferenceType, out Type modelType);
        bool TryGetReferenceType(Type resourceType, out Type resourceReferenceType);
        ConstructorInfo GetTokenRetrieverConstructor();
        ConstructorInfo GetBearerTokenAuthenticatorConstructor();
    }
}
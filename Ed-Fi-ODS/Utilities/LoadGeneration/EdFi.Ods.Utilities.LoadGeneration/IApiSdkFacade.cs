using System;
using System.Collections;
using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IApiSdkFacade
    {
        IEnumerable GetAll(Type modelType);
        IRestResponse Post(object body);
    }
}
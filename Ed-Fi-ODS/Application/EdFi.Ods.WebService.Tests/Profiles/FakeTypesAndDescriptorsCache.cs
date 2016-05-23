using System;
using EdFi.Common.Caching;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Entities.Common.Caching;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class FakeTypesAndDescriptorsCache : ITypesAndDescriptorsCache
    {
        public int GetId(string typeName, string value)
        {
            if (value == null)
                return 0;

            return Convert.ToInt32(value.Substring(1));
        }

        public string GetValue(string typeName, int id)
        {
            if (id == 0)
                return null;

            return typeName.Substring(0, 1) + id;
        }
    }
}
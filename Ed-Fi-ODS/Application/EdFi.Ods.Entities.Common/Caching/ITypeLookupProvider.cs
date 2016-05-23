using System.Collections.Generic;

namespace EdFi.Ods.Entities.Common.Caching
{
    public interface ITypeLookupProvider
    {
        TypeLookup GetSingleTypeLookupById(string typeName, int id);
        TypeLookup GetSingleTypeLookupByShortDescription(string typeName, string shortDescription);
        IDictionary<string, IList<TypeLookup>> GetAllTypeLookups();
    }
}
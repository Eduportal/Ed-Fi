using System.Collections.Generic;

namespace EdFi.Ods.Entities.Common.Caching
{
    public interface IDescriptorLookupProvider
    {
        DescriptorLookup GetSingleDescriptorLookupById(string descriptorName, int id);
        IList<DescriptorLookup> GetDescriptorLookupsByDescriptorName(string descriptorName);
        IDictionary<string, IList<DescriptorLookup>> GetAllDescriptorLookups();
    }
}
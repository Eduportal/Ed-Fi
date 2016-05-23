using System.Collections.Generic;
using System.Xml.Linq;

namespace EdFi.Ods.BulkLoad.Core.Dictionaries
{
    public interface IDbDictionary
    {
        void Add(string aggregateName, XElement element);
        IList<XElement> GetByAggregateName(string aggregateName);
        XElement GetByAggregateNameAndId(IEnumerable<string> aggregateNames, string id);
        void Dispose();
    }
}

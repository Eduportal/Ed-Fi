using System;
using System.Collections.Generic;
using System.Xml.Linq;
using EdFi.Ods.XmlShredding;

namespace EdFi.Ods.BulkLoad.Core
{
    public interface IIndexedXmlFileReader : INodeSearch, IDisposable
    {
        IEnumerable<XElement> GetNodesByEntity(string entity);
    }
}
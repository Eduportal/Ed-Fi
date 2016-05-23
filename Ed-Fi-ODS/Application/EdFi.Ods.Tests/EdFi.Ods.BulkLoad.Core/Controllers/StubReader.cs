namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    using global::EdFi.Ods.BulkLoad.Core;

    public class StubReader : IIndexedXmlFileReader
    {
        public StubReader(string filePath, Exception exceptionToThrow = null)
        {
            if (exceptionToThrow != null)
                throw exceptionToThrow;
        }

        public XElement GetElementById(IEnumerable<string> searchEntities, string id)
        {
            return new XElement(XName.Get("Stubby"));
        }

        public XElement FindElement(IEnumerable<string> searchEntities, Func<XElement, bool> searchCriteria)
        {
            throw new NotImplementedException();
        }

        public XElement FindForeignKeyElement(XElement startingElement, string serializedRefMap)
        {
            throw new NotImplementedException();
        }

        public XElement FindByPath(XElement containingElement, string xPath)
        {
            throw new NotImplementedException();
        }

        IEnumerable<XElement> IIndexedXmlFileReader.GetNodesByEntity(string entity)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}